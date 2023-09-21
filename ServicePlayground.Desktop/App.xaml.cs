using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServicePlayground.Desktop.HostBuilders;
using ServicePlayground.Desktop.ViewModels;
using ServicePlayground.Protobuf.Client;
using ServicePlayground.Protobuf.Server;

namespace ServicePlayground.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;

        public App()
        {
            host = Host.CreateDefaultBuilder()
                .AddViewModels()
                .ConfigureServices(services =>
                {
                    services.AddAutoMapper(Assembly.GetCallingAssembly(), Assembly.GetAssembly(typeof(ItemsService)));
                    services.AddSingleton(s => new MainWindow
                    {
                        DataContext = s.GetRequiredService<MainViewModel>()
                    });

                    services.AddSingleton<ItemsClient>();
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            host.Start();
            
            MainWindow= host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
            
            base.OnStartup(e);
        }
    }
}