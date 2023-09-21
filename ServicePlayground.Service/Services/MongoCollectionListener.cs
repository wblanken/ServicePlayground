using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common;
using ServicePlayground.Common.Model;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public interface IDbService<TCollection>
{
    public Task<List<TCollection>> GetAllAsync();
}

public abstract class MongoCollectionListener<TCollection, TWorker> : BackgroundService, IDbService<TCollection>
    where TCollection : MongoItem
    where TWorker : MongoCollectionListener<TCollection, TWorker>
{
    private readonly ILogger<TWorker> logger;
    protected readonly IMongoContext dbContext;
    private readonly IMemoryCache memoryCache;

    protected MongoCollectionListener(ILogger<TWorker> logger, IMongoContext dbContext, IMemoryCache memoryCache)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
    }

    protected ILogger<TWorker> Logger => logger;
    
    public event Action<MongoCollectionChange<TCollection>>? OnMongoCollectionChanged;
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        await GetAllAsync();
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
        var errored = false;
        if (memoryCache.TryGetValue(MongoItem.GetCollectionName<TCollection>(), out List<TCollection> cacheList))
        {
            switch (change.OperationType)
            {
                case OperationType.Insert:
                    var addedItem = cacheList.FirstOrDefault(f => f.Id == change.Id);
                    if (addedItem is not null)
                    {
                        Logger.LogWarning($"Item with id {change.Id} aready exists in the cache!");
                        errored = true;
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
                        errored = true;
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
                        errored = true;
                    }
                    break;
                // case OperationType.Replace:
                //     break;
                // case OperationType.Invalidate:
                //     break;
                default:
                    errored = true;
                    Logger.LogError($"Operation type {change.OperationType.ToString()} not implemented!");
                    break;
            }
        }

        if (!errored)
        {
            OnMongoCollectionChanged?.Invoke(change);
        }
    }
    
    public async Task<List<TCollection>> GetAllAsync()
    {
        if (!memoryCache.TryGetValue("items", out List<TCollection> cacheList))
        {
            cacheList = await dbContext.GetAllAsync<TCollection>();
            memoryCache.Set(MongoItem.GetCollectionName<TCollection>(), cacheList);
        }
        
        return cacheList;
    }
}