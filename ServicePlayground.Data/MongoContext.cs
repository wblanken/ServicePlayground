using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ServicePlayground.Common;
using ServicePlayground.Common.Model;

namespace ServicePlayground.Data;

public interface IMongoContext
{
    public event EventHandler<ItemsChangedEventArgs> ItemsCollectionChanged;
    public Task<List<Item>> GetAllItemsAsync();
}

public class MongoContext : IMongoContext
{
    private readonly ILogger<MongoContext> logger;
    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<Item> items;

    private Thread itemsWatcher;
    
    public MongoContext(ILogger<MongoContext> logger, string connectionString, string dbName)
    {
        this.logger = logger;
        
        if (string.IsNullOrEmpty(connectionString))
        {
            this.logger.LogError($"Connection string arg cannot be empty!");
            throw new ArgumentException($"Connection string cannot be empty!", nameof(connectionString));
        }
        if (string.IsNullOrEmpty(dbName))
        {
            this.logger.LogError($"Database arg cannot be empty!");
            throw new ArgumentException($"Database cannot be empty!", nameof(dbName));
        }
        
        BsonClassMap.RegisterClassMap<Item>(cm =>
        {
            cm.AutoMap();
            cm.MapIdField(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        
        client = new MongoClient(connectionString);
        database = client.GetDatabase(dbName);
        items = database.GetCollection<Item>(nameof(items));
        
        itemsWatcher = new Thread(StartItemsWatch);
        itemsWatcher.Start();
    }

    public event EventHandler<ItemsChangedEventArgs> ItemsCollectionChanged;

    public async Task<List<Item>> GetAllItemsAsync()
    {
        var filter = Builders<Item>.Filter.Empty;
        return await items.Find(filter).ToListAsync();
    }

    private void StartItemsWatch()
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
            ShowExpandedEvents = false
        };
            
        using (var cursor = items.Watch(options))
        {
            foreach (var changedItem in cursor.ToEnumerable())
            {
                logger.LogInformation($"Change detected: {changedItem.OperationType} in the Items collection.");
                
                var eventArgs = new ItemsChangedEventArgs
                {
                    OperationType = OperationType.Unkown,
                    Item = new Item()
                };
                
                switch (changedItem.OperationType)
                {
                    case ChangeStreamOperationType.Insert:
                        eventArgs.OperationType = OperationType.Insert;
                        eventArgs.Item = changedItem.FullDocument;
                        break;
                    
                    case ChangeStreamOperationType.Update:
                        eventArgs.OperationType = OperationType.Update;
                        eventArgs.Item = changedItem.FullDocument;
                        break;
                    
                    case ChangeStreamOperationType.Delete:
                        eventArgs.OperationType = OperationType.Delete;
                        eventArgs.Item.Id = changedItem.DocumentKey["_id"].ToString();
                        break;
                    
                    // case ChangeStreamOperationType.Replace:
                    //     eventArgs.OperationType = OperationType.Replace;
                    //     break;
                    
                    // case ChangeStreamOperationType.Invalidate:
                    //     eventArgs.OperationType = OperationType.Invalidate;
                    //     break;
                    default:
                        logger.LogError($"Operation type {changedItem.OperationType.ToString()} not implemented!");
                        break;
                        // throw new NotImplementedException($"Operation type {changedItem.OperationType.ToString()} not implemented!");
                }
                    
                ItemsCollectionChanged?.Invoke(this, eventArgs);
            }
        }
    }
}