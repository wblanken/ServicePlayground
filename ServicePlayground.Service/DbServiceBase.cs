using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public abstract class DbServiceBase<TCollection, TWorker> : BackgroundService, IDbService<TCollection>
    where TCollection : MongoItem
    where TWorker : DbServiceBase<TCollection, TWorker>
{
    protected ILogger<TWorker> Logger { get; }
    protected MongoContext DbContext { get; }
    protected IMemoryCache MemoryCache { get; }

    protected DbServiceBase(ILogger<TWorker> logger, MongoContext dbContext, IMemoryCache memoryCache)
    {
       Logger = logger;
       DbContext = dbContext;
       MemoryCache = memoryCache;
    }
    
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
            await DbContext.StartWatch<TCollection>(OnCollectionItemChanged);
        }
    }

    protected virtual void OnCollectionItemChanged(MongoCollectionChange<TCollection> change)
    {
        var errored = false;
        if (MemoryCache.TryGetValue(MongoItem.GetCollectionName<TCollection>(), out List<TCollection>? cacheList))
        {
            if(cacheList != null)
            {
                switch (change.OperationType)
                {
                    case OperationType.Insert:
                        var addedItem = cacheList.FirstOrDefault(f => f.Id == change.Id);
                        if (addedItem is not null)
                        {
                            Logger.LogWarning($"Item with id {change.Id} already exists in the cache!");
                            errored = true;
                        }
                        else
                        {
                            Logger.LogInformation($"Adding with id {change.Id} to the cache!");
                            if (change.ChangedItem != null) cacheList.Add(change.ChangedItem);
                        }

                        break;

                    case OperationType.Update:
                        var updatedItem = cacheList.FirstOrDefault(f => f.Id == change.Id);
                        if (updatedItem is not null)
                        {
                            Logger.LogInformation($"Updating item with id {change.Id} int the cache!");
                            cacheList.Remove(updatedItem);
                            if (change.ChangedItem != null) cacheList.Add(change.ChangedItem);
                        }
                        else
                        {
                            Logger.LogWarning($"Item with id {change.Id} not found in cache to update!");
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
                            Logger.LogWarning($"Item with id {change.Id} not found in cache to remove!");
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
        }

        if (!errored)
        {
            OnMongoCollectionChanged?.Invoke(change);
        }
    }
    
    public virtual async Task<List<TCollection>?> GetAllAsync()
    {
        if (!MemoryCache.TryGetValue(MongoItem.GetCollectionName<TCollection>(), out Dictionary<string, TCollection>? cacheList))
        {
            Logger.LogInformation($"{MongoItem.GetCollectionName<TCollection>()} Collection not found in cache. Caching...");
            var result = await DbContext.GetAllAsync<TCollection>();
            cacheList = result.ToDictionary(k => k.Id);
            MemoryCache.Set(MongoItem.GetCollectionName<TCollection>(), cacheList);
        }
        
        return cacheList?.Values.ToList();
    }

    public virtual async Task<TCollection?> GetAsync(string id)
    {
        if (!MemoryCache.TryGetValue(MongoItem.GetCollectionName<TCollection>(), out Dictionary<string, TCollection>? cacheList))
        {
            Logger.LogInformation($"{MongoItem.GetCollectionName<TCollection>()} Collection not found in cache. Caching...");
            var result = await DbContext.GetAllAsync<TCollection>();
            cacheList = result.ToDictionary(k => k.Id);
            MemoryCache.Set(MongoItem.GetCollectionName<TCollection>(), cacheList);
        }

        TCollection? foundEntry = null;
        if (!cacheList?.TryGetValue(id, out foundEntry) ?? false)
        {
            Logger.LogError($"No entry with id: {id} found in collection {MongoItem.GetCollectionName<TCollection>()}!");
        }
            
        return foundEntry;
    }

    public virtual async Task CreateAsync([NotNull] TCollection newEntry)
    {
        throw new NotImplementedException();
    }
    
    public virtual async Task UpdateAsync(string id, [NotNull] TCollection updatedEntry)
    {
        throw new NotImplementedException();
    }
    
    public virtual async Task RemoveAsync(string id)
    {
        throw new NotImplementedException();
    }
}