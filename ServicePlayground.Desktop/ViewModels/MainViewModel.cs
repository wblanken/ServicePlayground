using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServicePlayground.Common.Model;
using ServicePlayground.Protobuf.Client;

namespace ServicePlayground.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ItemsClient ItemsClient { get; }
    public static ObservableCollection<Item>? Items { get; private set; }

    private MainViewModel(ILogger<MainViewModel> logger, ItemsClient itemsClient) : base(logger)
    {
        ItemsClient = itemsClient;
    }
    
    public static MainViewModel LoadViewModel(ILogger<MainViewModel> logger, ItemsClient itemsClient)
    {
        var viewModel = new MainViewModel(logger, itemsClient);
        
        // If this was a real app we'd start the data load without blocking. IDC for this example
        var result = Task.Run(itemsClient.GetItemsAsync).Result;
        Items = new ObservableCollection<Item>(result);
        
        return viewModel;
    }

    private async Task ItemChangeListener()
    {
        /*
            var client = new Greet.GreeterClient(channel);
            using var call = client.SayHellos(new HelloRequest { Name = "World" });

            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("Greeting: " + response.Message);
                // "Greeting: Hello World" is written multiple times
            }
         */
    }
}