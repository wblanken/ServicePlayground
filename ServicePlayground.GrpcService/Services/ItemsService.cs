using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using ServicePlayground.GrpcService;

namespace ServicePlayground.GrpcService.Services;

public class ItemsService : Items.ItemsBase
{
    private readonly ILogger<GreeterService> logger;
    private readonly IMemoryCache memoryCache;
    
    public ItemsService(ILogger<GreeterService> logger, IMemoryCache memoryCache)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
    }

    public override async Task<GetItemsResponse> GetItems(GetItemsRequest request, ServerCallContext context)
    {
        if (memoryCache.TryGetValue("items", out List<Common.Model.Item> items))
        {
            var response = new GetItemsResponse();
            response.Items.AddRange(items);
        }
    }
}