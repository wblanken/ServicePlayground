using ServicePlayground.Data;
using ServicePlayground.Service;

var builder = Host.CreateApplicationBuilder();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddHostedService<Worker>();

var databaseSettings = config.GetRequiredSection("DatabaseSettings");
var connectionString = databaseSettings["ConnectionString"];
var dbName = databaseSettings["Database"];

builder.Services.AddSingleton<IMongoContext>(e => new MongoContext(e.GetService<ILogger<MongoContext>>(), connectionString, dbName));
builder.Services.AddMemoryCache();

var host = builder.Build();
host.Run();