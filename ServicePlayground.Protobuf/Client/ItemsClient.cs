using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServicePlayground.Protobuf.Proto;

namespace ServicePlayground.Protobuf.Client;

public partial class ItemsClient : ClientBase
{
    public ItemsClient(ILogger<ItemsClient> logger, IMapper mapper, IConfiguration configuration) 
        : base(logger, mapper, configuration, "PlaygroundService")
    { }
    
    public async Task<List<Common.Model.Item>> GetItemsAsync()
    {
        using var channel = GrpcChannel.ForAddress(ServiceUrl);
        var client = new Items.ItemsClient(channel);
        var response = await client.GetItemsAsync(new Empty());
        return Mapper.Map<List<Common.Model.Item>>(response.Items);
    }
}