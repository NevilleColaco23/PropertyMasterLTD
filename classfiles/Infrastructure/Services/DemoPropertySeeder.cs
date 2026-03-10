using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using MyWarehouse.Domain.Property;

namespace MyWarehouse.Infrastructure.Services;

/// <summary>
/// Service to seed demo property data from MongoDB configuration/admin panel
/// Instead of hardcoding, demo data is stored in DB and can be modified
/// </summary>
public class DemoPropertySeeder
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<DemoPropertySeeder> _logger;
    private const int DEMO_PROPERTY_ID = -1;

    public DemoPropertySeeder(
        IMongoDatabase mongoDatabase,
        ILogger<DemoPropertySeeder> logger)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    /// <summary>
    /// Seeds demo property from database template
    /// This allows admins to customize demo data via database
    /// </summary>
    public async Task SeedDemoPropertyIfNotExistsAsync()
    {
        try
        {
            var propertiesCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Property");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", DEMO_PROPERTY_ID);
            
            var existingProperty = await propertiesCollection.Find(filter).FirstOrDefaultAsync();
            
            if (existingProperty != null)
            {
                _logger.LogInformation("Demo property already exists, skipping seed");
                return;
            }

            // Check if demo template exists in DemoPropertyTemplate collection
            var templateCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("DemoPropertyTemplate");
            var template = await templateCollection.Find(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Empty).FirstOrDefaultAsync();

            if (template == null)
            {
                _logger.LogWarning("No demo property template found in database. Creating default template.");
                await CreateDefaultTemplateAsync();
                template = await templateCollection.Find(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Empty).FirstOrDefaultAsync();
            }

            // Create demo property from template
            await CreateDemoPropertyFromTemplateAsync(template);
            
            _logger.LogInformation("Demo property seeded successfully from database template");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed demo property");
            throw;
        }
    }

    private async Task CreateDefaultTemplateAsync()
    {
        var templateCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("DemoPropertyTemplate");
        
        var defaultTemplate = new MongoDB.Bson.BsonDocument
        {
            { "_id", 1 },
            { "PropertyName", "The Grand Hotel - Demo" },
            { "PropertyCode", "DEMO0001" },
            { "CompanyLogoURL", "https://placehold.co/200x200/4CAF50/white?text=DEMO" },
            { "Rooms", new MongoDB.Bson.BsonArray
            {
                new MongoDB.Bson.BsonDocument { { "RoomCode", "101" }, { "RoomName", "Deluxe King Room" }, { "LogoURL", "https://placehold.co/400x300/2196F3/white?text=Room+101" } },
                new MongoDB.Bson.BsonDocument { { "RoomCode", "102" }, { "RoomName", "Deluxe Queen Room" }, { "LogoURL", "https://placehold.co/400x300/2196F3/white?text=Room+102" } },
                new MongoDB.Bson.BsonDocument { { "RoomCode", "201" }, { "RoomName", "Executive Suite" }, { "LogoURL", "https://placehold.co/400x300/9C27B0/white?text=Suite+201" } },
                new MongoDB.Bson.BsonDocument { { "RoomCode", "202" }, { "RoomName", "Presidential Suite" }, { "LogoURL", "https://placehold.co/400x300/9C27B0/white?text=Suite+202" } },
                new MongoDB.Bson.BsonDocument { { "RoomCode", "301" }, { "RoomName", "Family Room" }, { "LogoURL", "https://placehold.co/400x300/FF9800/white?text=Family+301" } },
                new MongoDB.Bson.BsonDocument { { "RoomCode", "302" }, { "RoomName", "Ocean View Room" }, { "LogoURL", "https://placehold.co/400x300/00BCD4/white?text=Ocean+302" } }
            }},
            { "Bookings", new MongoDB.Bson.BsonArray
            {
                new MongoDB.Bson.BsonDocument { { "GuestName", "John Smith" }, { "RoomCode", "101" }, { "Status", "Completed" }, { "DaysOffset", -10 }, { "Duration", 3 } },
                new MongoDB.Bson.BsonDocument { { "GuestName", "Sarah Johnson" }, { "RoomCode", "201" }, { "Status", "Active" }, { "DaysOffset", -2 }, { "Duration", 4 } },
                new MongoDB.Bson.BsonDocument { { "GuestName", "Michael Brown" }, { "RoomCode", "302" }, { "Status", "Confirmed" }, { "DaysOffset", 5 }, { "Duration", 7 } },
                new MongoDB.Bson.BsonDocument { { "GuestName", "Emma Davis" }, { "RoomCode", "102" }, { "Status", "Pending" }, { "DaysOffset", 10 }, { "Duration", 4 } }
            }},
            { "Menus", new MongoDB.Bson.BsonArray
            {
                new MongoDB.Bson.BsonDocument 
                { 
                    { "MenuName", "Breakfast Menu" }, 
                    { "MenuType", "Breakfast" },
                    { "AvailableFrom", "06:00" },
                    { "AvailableTo", "11:00" },
                    { "Items", new MongoDB.Bson.BsonArray 
                    {
                        new MongoDB.Bson.BsonDocument { { "Name", "Continental Breakfast" }, { "Price", 12.99M }, { "Description", "Croissant, jam, butter, coffee/tea" } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Full English Breakfast" }, { "Price", 18.99M }, { "Description", "Eggs, bacon, sausage, beans, toast" } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Pancake Stack" }, { "Price", 14.99M }, { "Description", "Stack of 3 with maple syrup" } }
                    }}
                },
                new MongoDB.Bson.BsonDocument 
                { 
                    { "MenuName", "Lunch Menu" }, 
                    { "MenuType", "Lunch" },
                    { "AvailableFrom", "12:00" },
                    { "AvailableTo", "15:00" },
                    { "Items", new MongoDB.Bson.BsonArray 
                    {
                        new MongoDB.Bson.BsonDocument { { "Name", "Caesar Salad" }, { "Price", 16.99M }, { "Description", "Romaine lettuce, parmesan, croutons" } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Grilled Chicken Sandwich" }, { "Price", 19.99M }, { "Description", "With fries and coleslaw" } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Fish & Chips" }, { "Price", 22.99M }, { "Description", "Battered cod with chunky chips" } }
                    }}
                },
                new MongoDB.Bson.BsonDocument 
                { 
                    { "MenuName", "Dinner Menu" }, 
                    { "MenuType", "Dinner" },
                    { "AvailableFrom", "18:00" },
                    { "AvailableTo", "22:00" },
                    { "Items", new MongoDB.Bson.BsonArray 
                    {
                        new MongoDB.Bson.BsonDocument { { "Name", "Ribeye Steak" }, { "Price", 34.99M }, { "Description", "12oz ribeye with vegetables" } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Grilled Salmon" }, { "Price", 28.99M }, { "Description", "Atlantic salmon with lemon butter" } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Vegetarian Pasta" }, { "Price", 22.99M }, { "Description", "Penne with roasted vegetables" } }
                    }}
                }
            }},
            { "CreatedAt", DateTime.UtcNow },
            { "Notes", "This template is used to seed demo property. Modify this document to change demo data without code changes." }
        };

        await templateCollection.InsertOneAsync(defaultTemplate);
        _logger.LogInformation("Created default demo property template in database");
    }

    private async Task CreateDemoPropertyFromTemplateAsync(MongoDB.Bson.BsonDocument template)
    {
        var propertiesCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Property");
        var bookingsCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Bookings");
        var menusCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Menus");

        // Create property
        var rooms = template["Rooms"].AsBsonArray.Select(r => new MongoDB.Bson.BsonDocument
        {
            { "_id", $"room-{r["RoomCode"].AsString}" },
            { "RoomCode", r["RoomCode"].AsString },
            { "RoomName", r["RoomName"].AsString },
            { "Active", true },
            { "CompanyLogoURL", r["LogoURL"].AsString }
        }).ToList();

        var propertyDoc = new MongoDB.Bson.BsonDocument
        {
            { "_id", DEMO_PROPERTY_ID },
            { "Name", template["PropertyName"].AsString },
            { "Active", true },
            { "PropertyCode", template["PropertyCode"].AsString },
            { "CompanyLogoURL", template["CompanyLogoURL"].AsString },
            { "Rooms", new MongoDB.Bson.BsonArray(rooms) },
            { "CreatedAt", DateTime.UtcNow },
            { "IsDemo", true }
        };

        await propertiesCollection.InsertOneAsync(propertyDoc);

        // Create bookings from template
        var bookings = new List<MongoDB.Bson.BsonDocument>();
        int bookingId = -1;
        foreach (var booking in template["Bookings"].AsBsonArray)
        {
            var daysOffset = booking["DaysOffset"].AsInt32;
            var duration = booking["Duration"].AsInt32;
            
            bookings.Add(new MongoDB.Bson.BsonDocument
            {
                { "_id", bookingId-- },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "RoomCode", booking["RoomCode"].AsString },
                { "GuestName", booking["GuestName"].AsString },
                { "GuestEmail", $"{booking["GuestName"].AsString.Replace(" ", ".").ToLower()}@example.com" },
                { "CheckIn", DateTime.UtcNow.AddDays(daysOffset) },
                { "CheckOut", DateTime.UtcNow.AddDays(daysOffset + duration) },
                { "Status", booking["Status"].AsString },
                { "TotalAmount", 150.00M * duration },
                { "CreatedAt", DateTime.UtcNow.AddDays(daysOffset - 5) },
                { "IsDemo", true }
            });
        }
        await bookingsCollection.InsertManyAsync(bookings);

        // Create menus from template
        var menus = new List<MongoDB.Bson.BsonDocument>();
        int menuId = -1;
        foreach (var menu in template["Menus"].AsBsonArray)
        {
            menus.Add(new MongoDB.Bson.BsonDocument
            {
                { "_id", menuId-- },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "MenuName", menu["MenuName"].AsString },
                { "MenuType", menu["MenuType"].AsString },
                { "Active", true },
                { "AvailableFrom", menu["AvailableFrom"].AsString },
                { "AvailableTo", menu["AvailableTo"].AsString },
                { "Items", menu["Items"].AsBsonArray },
                { "CreatedAt", DateTime.UtcNow },
                { "IsDemo", true }
            });
        }
        await menusCollection.InsertManyAsync(menus);

        _logger.LogInformation("Created demo property, {BookingCount} bookings, and {MenuCount} menus from template", 
            bookings.Count, menus.Count);
    }
}
