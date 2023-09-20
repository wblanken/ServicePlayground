using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServicePlayground.Desktop.ViewModels;
using ServicePlayground.Protobuf.Client;

namespace ServicePlayground.Desktop.HostBuilders;

public static class AddViewModelsHostBuilderExtensions
{
    public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddTransient(CreateMainViewModel);
        });
        return hostBuilder;
    }
    
    private static MainViewModel CreateMainViewModel(IServiceProvider services)
    {
        return MainViewModel.LoadViewModel(services.GetRequiredService<ILogger<MainViewModel>>(), services.GetRequiredService<ItemsClient>());
    }
}