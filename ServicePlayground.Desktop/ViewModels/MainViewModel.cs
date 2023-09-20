
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServicePlayground.Common.Model;
using ServicePlayground.Protobuf.Client;

namespace ServicePlayground.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ILogger<MainViewModel> logger;
    private readonly ItemsClient itemsClient;
    
    public static ObservableCollection<Item> Items { get; set; }

    private MainViewModel(ILogger<MainViewModel> logger, ItemsClient itemsClient)
    {
        this.logger = logger;
        this.itemsClient = itemsClient;
    }
    
    public static MainViewModel LoadViewModel(ILogger<MainViewModel> logger, ItemsClient itemsClient)
    {
        var viewModel = new MainViewModel(logger, itemsClient);
        
        // If this was a real app we'd start the data load without blocking. IDC for this example
        var result = Task.Run(itemsClient.GetItemsAsync).Result;
        Items = new ObservableCollection<Item>(result);
        
        return viewModel;
    }
}