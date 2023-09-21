using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ServicePlayground.Common;
using ServicePlayground.Common.Model;

namespace ServicePlayground.Data;

public class MongoContext
{
    private ILogger<MongoContext> Logger { get; }
    private readonly IMongoDatabase database;

    public MongoContext(ILogger<MongoContext> logger, IOptions<DatabaseSettings> databaseSettings)
    {
        Logger = logger;
        var connectionString = databaseSettings.Value.ConnectionString;
        var dbName = databaseSettings.Value.DatabaseName;

        if (string.IsNullOrEmpty(connectionString))
        {
            var ex = new ArgumentException("Connection string cannot be empty!", nameof(databaseSettings));
            Logger.LogCritical(ex.Message);
            throw ex;
        }
        
        if (string.IsNullOrEmpty(dbName))
        {
            var ex = new ArgumentException("Database name cannot be empty!", nameof(databaseSettings));
            Logger.LogCritical(ex.Message);
            throw ex;
        }

        BsonClassMap.RegisterClassMap<MongoItem>(cm =>
        {
            cm.AutoMap();
            cm.MapIdField(c => c.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        BsonClassMap.RegisterClassMap<Item>();
        
        var client = new MongoClient(connectionString);
        database = client.GetDatabase(dbName);
    }

    public async Task<List<TCollection>> GetAllAsync<TCollection>()
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        var filter = Builders<TCollection>.Filter.Empty;
        return await collection.Find(filter).ToListAsync();
    }

    public async Task<TCollection> GetAsync<TCollection>(string id)
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync<TCollection>(TCollection newEntry)
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        await collection.InsertOneAsync(newEntry);
    }

    public async Task UpdateAsync<TCollection>(string id, TCollection updatedEntry)
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        var result = await collection.ReplaceOneAsync(x => x.Id == id, updatedEntry);
    }

    public async Task RemoveAsync<TCollection>(string id)
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
       var result = await collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task StartWatch<TCollection>(Action<MongoCollectionChange<TCollection>> onCollectionItemChanged)
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        
        using var cursor = await collection.WatchAsync(options: new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup });
        await cursor.ForEachAsync(changedItem =>
        {
            Logger.LogInformation(
                $"Change detected: {changedItem.OperationType} in the {collection.CollectionNamespace.CollectionName} collection.");

            var change = new MongoCollectionChange<TCollection>()
            {
                Id = changedItem.DocumentKey["_id"].ToString()
            };

            switch (changedItem.OperationType)
            {
                case ChangeStreamOperationType.Insert:
                    change.OperationType = OperationType.Insert;
                    change.ChangedItem = changedItem.FullDocument;
                    break;

                case ChangeStreamOperationType.Update:
                    change.OperationType = OperationType.Update;
                    change.ChangedItem = changedItem.FullDocument;
                    break;

                case ChangeStreamOperationType.Delete:
                    change.OperationType = OperationType.Delete;
                    break;

                // case ChangeStreamOperationType.Replace:
                //     eventArgs.OperationType = OperationType.Replace;
                //     break;

                // case ChangeStreamOperationType.Invalidate:
                //     eventArgs.OperationType = OperationType.Invalidate;
                //     break;
                default:
                    Logger.LogError($"Operation type {changedItem.OperationType.ToString()} not implemented!");
                    break;
            }
            
            onCollectionItemChanged(change);
        });
    }

    private IMongoCollection<TCollection> GetCollection<TCollection>()
        where TCollection : MongoItem
    {
        var collectionName = MongoItem.GetCollectionName<TCollection>();
        return database.GetCollection<TCollection>(collectionName);
    }
}