using Microsoft.Extensions.Options;
using MongoDB.Driver;
using AccessLogWorker;
using Messaging.Shared;

var builder = Host.CreateApplicationBuilder(args);

// Bind RabbitMQ options from configuration (appsettings or environment).
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

// Register MongoDB client using environment variable for URI.
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var mongoUri = builder.Configuration["MONGODB_URI"];
    if (string.IsNullOrEmpty(mongoUri))
        throw new InvalidOperationException("MONGODB_URI environment variable must be set.");
    return new MongoClient(mongoUri);
});

// Register IMongoDatabase, pulling the DB name from the URI (defaults to 'ListingDB').
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var mongoUri = builder.Configuration["MONGODB_URI"];
    var mongoUrl = new MongoUrl(mongoUri);
    var dbName = mongoUrl.DatabaseName ?? "ListingDB";
    return client.GetDatabase(dbName);
});

// Register the RabbitMQ consumer worker service.
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();