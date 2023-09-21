using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common.Model;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public class ItemsDbService : MongoCollectionListener<Item, ItemsDbService>
{
    public ItemsDbService(ILogger<ItemsDbService> logger, IMongoContext dbContext, IMemoryCache memoryCache)
        : base(logger, dbContext, memoryCache)
    {}
}