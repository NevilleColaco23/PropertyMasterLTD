using AutoMapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Services;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // MongoDB client
        services.AddSingleton<IMongoClient>(_ =>
        {
            var mongoUri = context.Configuration["MONGODB_URI"];
            if (string.IsNullOrEmpty(mongoUri))
                throw new InvalidOperationException("MONGODB_URI must be set in local.settings.json or application settings.");

            var client = new MongoClient(mongoUri);

            // Verify connection on startup
            client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));

            return client;
        });

        // MongoDB database (database name extracted from the URI)
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var mongoUri = context.Configuration["MONGODB_URI"]!;
            var mongoUrl = new MongoUrl(mongoUri);
            var dbName = mongoUrl.DatabaseName ?? "ListingDB";
            return client.GetDatabase(dbName);
        });

        // AutoMapper (empty config — repository needs IMapper but Function doesn't use mappings)
        services.AddSingleton<IMapper>(_ =>
        {
            var config = new MapperConfiguration(cfg => { });
            return config.CreateMapper();
        });

        // Counter service for MongoDB auto-incrementing IDs
        services.AddSingleton<ICounterService, CounterService>();

        // UserActivity repository (scoped — one instance per function invocation)
        services.AddScoped<IUserActivityRepository, UserActivityRepositoryMongo>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
