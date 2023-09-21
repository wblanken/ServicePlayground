using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.Common.Model;
using ServicePlayground.Data;

namespace ServicePlayground.Service;

public class ItemsDbService : DbServiceBase<Item, ItemsDbService>
{
    public ItemsDbService(ILogger<ItemsDbService> logger, MongoContext dbContext, IMemoryCache memoryCache)
        : base(logger, dbContext, memoryCache)
    {}
}