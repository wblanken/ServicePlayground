using System.Reflection;
using Microsoft.Extensions.Configuration;
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
    public Task<List<TCollection>> GetAllAsync<TCollection>() 
        where TCollection : MongoItem;
    public Task StartWatch<TCollection>(Action<MongoCollectionChange<TCollection>> onCollectionItemChanged) 
        where TCollection : MongoItem;
}

public class MongoContext : IMongoContext
{
    private readonly ILogger<MongoContext> logger;
    private readonly MongoClient client;
    private readonly IMongoDatabase database;

    public MongoContext(ILogger<MongoContext> logger, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetRequiredSection("DatabaseSettings");
        var connectionString = databaseSettings["ConnectionString"];
        var dbName = databaseSettings["Database"];

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

        BsonClassMap.RegisterClassMap<MongoItem>(cm =>
        {
            cm.AutoMap();
            cm.MapIdField(c => c.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        BsonClassMap.RegisterClassMap<Item>();
        
        this.logger = logger;
        client = new MongoClient(connectionString);
        database = client.GetDatabase(dbName);
    }

    public async Task<List<TCollection>> GetAllAsync<TCollection>()
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        var filter = Builders<TCollection>.Filter.Empty;
        return await collection.Find(filter).ToListAsync();
    }

    public async Task StartWatch<TCollection>(Action<MongoCollectionChange<TCollection>> onCollectionItemChanged)
        where TCollection : MongoItem
    {
        var collection = GetCollection<TCollection>();
        
        using var cursor = await collection.WatchAsync(options: new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup });
        await cursor.ForEachAsync(changedItem =>
        {
            logger.LogInformation(
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
                    logger.LogError($"Operation type {changedItem.OperationType.ToString()} not implemented!");
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