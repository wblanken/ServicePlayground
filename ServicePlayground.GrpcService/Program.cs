using System.Reflection;
using AutoMapper;
using ServicePlayground.Data;
using ServicePlayground.Protobuf.Server;
using ServicePlayground.Service;
using ServicePlayground.Common;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddEnvironmentVariables();
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IMongoContext, MongoContext>();
builder.Services.AddSingleton<IDbService<ServicePlayground.Common.Model.Item>, ItemsDbService>();
builder.Services.AddHostedService<ItemsDbService>();
builder.Services.AddAutoMapper(Assembly.GetCallingAssembly(), Assembly.GetAssembly(typeof(ItemsService)));

builder.Services.AddTransient(s => 
    ItemsService.ServiceFactory(
        s.GetRequiredService<ILogger<ItemsService>>(),
        s.GetRequiredService<IDbService<ServicePlayground.Common.Model.Item>>(),
        s.GetRequiredService<IMapper>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ItemsService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapGrpcReflectionService();

app.Run();