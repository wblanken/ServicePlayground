using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServicePlayground.Protobuf.Proto;

namespace ServicePlayground.Protobuf.Server;

public class ItemsService : Items.ItemsBase
{
    private readonly ILogger<ItemsService> logger;
    private readonly IMemoryCache memoryCache;
    private readonly IMapper mapper;
    
    public ItemsService(ILogger<ItemsService> logger, IMemoryCache memoryCache, IMapper mapper)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.mapper = mapper;
    }

    public override Task<GetItemsResponse> GetItems(GetItemsRequest request, ServerCallContext context)
    {
        var response = new GetItemsResponse();
        if (memoryCache.TryGetValue("items", out List<Common.Model.Item> items))
        {
            var responseItems = mapper.Map<List<Item>>(items);
            response.Items.AddRange(responseItems);
        }
        
        return Task.FromResult(response);
    }
}