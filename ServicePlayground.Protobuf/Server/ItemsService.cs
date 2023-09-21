using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ServicePlayground.Protobuf.Proto;
using ServicePlayground.Service;

namespace ServicePlayground.Protobuf.Server;

public class ItemsService : Items.ItemsBase, IService<Common.Model.Item, ItemsService>
{
    public ILogger<ItemsService> Logger { get; }
    public IDbService<Common.Model.Item> DbService { get; }
    public IMapper Mapper { get; }

    private ItemsService(ILogger<ItemsService> logger, IDbService<Common.Model.Item> dbService, IMapper mapper)
    {
       Logger = logger;
       DbService = dbService;
       Mapper = mapper;
    }

    public static ItemsService ServiceFactory(ILogger<ItemsService> logger, IDbService<Common.Model.Item> dbService, IMapper mapper)
    {
        return new ItemsService(logger, dbService, mapper);
    }
    
    public override async Task<GetItemsResponse> GetItems(Empty request, ServerCallContext context)
    {
        var response = new GetItemsResponse();

        var items = await DbService.GetAllAsync();
        var responseItems = Mapper.Map<List<Item>>(items);
        response.Items.AddRange(responseItems);
        
        return response;
    }

    public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<SubscribeResponse> responseStream, ServerCallContext context)
    {
        var subscriber = request.EditorPdId; // We really don't care about this id, just need to see we have an active connection
        await AwaitCancellation(context.CancellationToken);
    }

    private static Task AwaitCancellation(CancellationToken token)
    {
        var completion = new TaskCompletionSource<object>();
        token.Register(() => completion.SetResult(null));
        return completion.Task;
    }
}