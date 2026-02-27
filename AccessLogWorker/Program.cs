using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using AccessLogWorker;
using Messaging.Shared;
using MyWarehouse.Application.Services;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;
using AutoMapper;

var builder = Host.CreateApplicationBuilder(args);

// Bind RabbitMQ options from configuration (appsettings or environment).
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

// Register MongoDB client using environment variable for URI.
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var mongoUri = builder.Configuration["MONGODB_URI"];
    if (string.IsNullOrEmpty(mongoUri))
        throw new InvalidOperationException("MONGODB_URI environment variable must be set.");

    Console.WriteLine($"?? Connecting to MongoDB: {mongoUri}");
    var client = new MongoClient(mongoUri);

    // Test connection
    try
    {
        client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        Console.WriteLine("? MongoDB connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? MongoDB connection failed: {ex.Message}");
        throw;
    }

    return client;
});

// Register IMongoDatabase, pulling the DB name from the URI (defaults to 'ListingDB').
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var mongoUri = builder.Configuration["MONGODB_URI"];
    var mongoUrl = new MongoUrl(mongoUri);
    var dbName = mongoUrl.DatabaseName ?? "ListingDB";

    Console.WriteLine($"?? Using MongoDB database: {dbName}");

    return client.GetDatabase(dbName);
});

// Register AutoMapper with minimal configuration (Worker doesn't use mappings, but repository constructor needs IMapper)
builder.Services.AddSingleton<IMapper>(sp =>
{
    var config = new MapperConfiguration(cfg =>
    {
        // Empty configuration - Worker only uses repository.Add() which doesn't need mappings
    });
    return config.CreateMapper();
});

// Register CounterService (for auto-incrementing IDs)
builder.Services.AddSingleton<ICounterService, CounterService>();

// Register AccessLogRepository (handles Add with CounterService)
builder.Services.AddScoped<IAccessLogRepository, AccessLogRepositoryMongo>();

// Register the RabbitMQ consumer worker service.
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();