using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public abstract class MongoCollectionListener<TCollection, TWorker> : BackgroundService
    where TCollection : MongoItem
    where TWorker : MongoCollectionListener<TCollection, TWorker>
{
    protected readonly ILogger<TWorker> logger;
    protected readonly IMongoContext dbContext;
    protected readonly IMemoryCache memoryCache;

    protected MongoCollectionListener(ILogger<TWorker> logger, IMongoContext dbContext, IMemoryCache memoryCache)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
    }

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
                case OperationType.Unkown:
                    break;
                case OperationType.Insert:
                    break;
                case OperationType.Update:
                    break;
                case OperationType.Replace:
                    break;
                case OperationType.Delete:
                    break;
                case OperationType.Invalidate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}