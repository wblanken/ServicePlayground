using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common;
using ServicePlayground.Common.Model;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly IMongoContext dbContext;
    private readonly IMemoryCache memoryCache;

    public ConcurrentDictionary<string, Item> itemsCache;
    
    public Worker(ILogger<Worker> logger, IMongoContext dbContext, IMemoryCache memoryCache)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        dbContext.ItemsCollectionChanged += OnItemsChanged;
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var items = await dbContext.GetAllItemsAsync();
        itemsCache = new ConcurrentDictionary<string, Item>(items.ToDictionary(k => k.Id, v => v));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            //logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        dbContext.ItemsCollectionChanged -= OnItemsChanged;
        return base.StopAsync(cancellationToken);
    }

    private void OnItemsChanged(object? sender, ItemsChangedEventArgs itemChange)
    {
        logger.LogInformation($"Incoming {itemChange.OperationType} operation for {{ ID: {itemChange.Item.Id} Name: {itemChange.Item.Name ?? string.Empty} }}");
        switch (itemChange.OperationType)
        {
            case OperationType.Insert:
            case OperationType.Update:
                itemsCache.AddOrUpdate(itemChange.Item.Id, itemChange.Item, (s, item) => item );
                break;
            
            case OperationType.Delete:
                itemsCache.TryRemove(itemChange.Item.Id, out var removedItem);
                break;
            
            // case OperationType.Replace:
            //     break;
            // case OperationType.Invalidate:
            //     break;
            
            default:
                logger.LogError($"{itemChange.OperationType} operation not implemented!");
                break;
        }
    }
}