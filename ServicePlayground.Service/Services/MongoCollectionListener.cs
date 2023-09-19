using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public abstract class MongoCollectionListener<TCollection, TWorker> : BackgroundService
    where TCollection : MongoItem
    where TWorker : MongoCollectionListener<TCollection, TWorker>
{
    private readonly ILogger<TWorker> logger;
    private readonly IMongoContext dbContext;
    private readonly IMemoryCache memoryCache;

    protected MongoCollectionListener(ILogger<TWorker> logger, IMongoContext dbContext, IMemoryCache memoryCache)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
    }

    protected ILogger<TWorker> Logger => logger;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        var cacheList = await dbContext.GetAllAsync<TCollection>();
        memoryCache.Set(MongoItem.GetCollectionName<TCollection>(), cacheList);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await dbContext.StartWatch<TCollection>(OnCollectionItemChanged);
            //await Task.Delay(1000, stoppingToken);
        }
    }

    protected virtual void OnCollectionItemChanged(MongoCollectionChange<TCollection> change)
    {
        if (memoryCache.TryGetValue(MongoItem.GetCollectionName<TCollection>(), out List<TCollection> cacheList))
        {
            switch (change.OperationType)
            {
                case OperationType.Insert:
                    var addedItem = cacheList.FirstOrDefault(f => f.Id == change.Id);
                    if (addedItem is not null)
                    {
                        Logger.LogWarning($"Item with id {change.Id} aready exists in the cache!");
                    }
                    else
                    {
                        Logger.LogInformation($"Adding with id {change.Id} to the cache!");
                        cacheList.Add(change.ChangedItem);    
                    }
                    break;
                
                case OperationType.Update:
                    var updatedItem = cacheList.FirstOrDefault(f => f.Id == change.Id);
                    if (updatedItem is not null)
                    {
                        Logger.LogInformation($"Updating item with id {change.Id} int the cache!");
                        cacheList.Remove(updatedItem);
                        cacheList.Add(change.ChangedItem);
                    }
                    else
                    {
                        logger.LogWarning($"Item with id {change.Id} not found in cache to update!");
                    }
                    break;
                
                case OperationType.Delete:
                    var removedItem = cacheList.FirstOrDefault(f => f.Id == change.Id);
                    if (removedItem != null)
                    {
                        Logger.LogInformation($"Removing item with id {change.Id} from cache!");
                        cacheList.Remove(removedItem);
                    }
                    else
                    {
                        logger.LogWarning($"Item with id {change.Id} not found in cache to remove!");
                    }
                    break;
                // case OperationType.Replace:
                //     break;
                // case OperationType.Invalidate:
                //     break;
                default:
                    Logger.LogError($"Operation type {change.OperationType.ToString()} not implemented!");
                    break;
            }
        }
    }
}