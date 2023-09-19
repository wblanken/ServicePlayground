using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common.Model;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public class ItemsListener : MongoCollectionListener<Item, ItemsListener>
{
    public ItemsListener(ILogger<ItemsListener> logger, IMongoContext dbContext, IMemoryCache memoryCache)
        : base(logger, dbContext, memoryCache)
    {}
}