using ServicePlayground.Data;
using ServicePlayground.Service;

var builder = Host.CreateApplicationBuilder();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var databaseSettings = config.GetRequiredSection("DatabaseSettings");
var connectionString = databaseSettings["ConnectionString"];
var dbName = databaseSettings["Database"];

// builder.Services.AddSingleton<IMongoContext>(e => new MongoContext(e.GetService<ILogger<MongoContext>>(), connectionString, dbName));
builder.Services.AddSingleton<IMongoContext, MongoContext>();
builder.Services.AddMemoryCache();

builder.Services.AddHostedService<ItemsListener>();

var host = builder.Build();
host.Run();