using AutoMapper;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

var builder = FunctionsApplication.CreateBuilder(args);

// Add user secrets so local dev secrets are picked up (in addition to local.settings.json)
builder.Configuration.AddUserSecrets<Program>(optional: true);

// MongoDB client — URI comes from user secrets / env var / Azure App Settings
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var mongoUri = builder.Configuration["MONGODB_URI"]
                   ?? throw new InvalidOperationException("MONGODB_URI must be set in configuration.");

    var client = new MongoClient(mongoUri);
    client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
    return client;
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client   = sp.GetRequiredService<IMongoClient>();
    var mongoUri = builder.Configuration["MONGODB_URI"]!;
    var dbName   = new MongoUrl(mongoUri).DatabaseName ?? "ListingDB";
    return client.GetDatabase(dbName);
});

// AutoMapper — empty config; required by UserActivityRepositoryMongo constructor
builder.Services.AddSingleton<IMapper>(_ =>
    new MapperConfiguration(_ => { }).CreateMapper());

// Counter service for MongoDB auto-increment IDs
builder.Services.AddSingleton<ICounterService, CounterService>();

// UserActivity repository (scoped — one per function invocation)
builder.Services.AddScoped<IUserActivityRepository, UserActivityRepositoryMongo>();

builder.Build().Run();
