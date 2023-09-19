using ServicePlayground.Data;
using ServicePlayground.GrpcService.Services;
using ServicePlayground.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddEnvironmentVariables();
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var databaseSettings = config.GetRequiredSection("DatabaseSettings");
var connectionString = databaseSettings["ConnectionString"];
var dbName = databaseSettings["Database"];

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IMongoContext, MongoContext>();
builder.Services.AddHostedService<ItemsListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();