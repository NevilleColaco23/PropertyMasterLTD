using MongoDB.Driver;
using MyWarehouse.Domain.Property;
using Microsoft.Extensions.Logging;

namespace MyWarehouse.Infrastructure.Services;

/// <summary>
/// Service to manage the shared demo property for new users
/// </summary>
public interface IDemoPropertyService
{
    Task<int> EnsureDemoPropertyExistsAsync();
    Task<bool> GrantUserAccessToDemoPropertyAsync(int userId);
}

public class DemoPropertyService : IDemoPropertyService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<DemoPropertyService> _logger;
    private const int DEMO_PROPERTY_ID = -1; // Special ID for demo property
    private const string DEMO_PROPERTY_NAME = "The Grand Hotel - Demo";
    private const string DEMO_PROPERTY_CODE = "DEMO0001";

    public DemoPropertyService(
        IMongoDatabase mongoDatabase,
        ILogger<DemoPropertyService> logger)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    /// <summary>
    /// Ensures the demo property exists in the database
    /// Creates it if it doesn't exist
    /// </summary>
    public async Task<int> EnsureDemoPropertyExistsAsync()
    {
        try
        {
            var propertiesCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Properties");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", DEMO_PROPERTY_ID);
            
            var existingProperty = await propertiesCollection.Find(filter).FirstOrDefaultAsync();
            
            if (existingProperty != null)
            {
                _logger.LogInformation("Demo property already exists with ID {PropertyId}", DEMO_PROPERTY_ID);
                return DEMO_PROPERTY_ID;
            }

            // Create demo property with sample data
            var demoProperty = CreateDemoProperty();
            
            var document = new MongoDB.Bson.BsonDocument
            {
                { "_id", DEMO_PROPERTY_ID },
                { "Name", demoProperty.Name },
                { "Active", true },
                { "PropertyCode", DEMO_PROPERTY_CODE },
                { "CompanyLogoURL", "https://placehold.co/200x200/4CAF50/white?text=DEMO" },
                { "Rooms", new MongoDB.Bson.BsonArray(demoProperty.Rooms.Select(r => new MongoDB.Bson.BsonDocument
                {
                    { "_id", r.Id },
                    { "RoomCode", r.RoomCode },
                    { "RoomName", r.RoomName },
                    { "Active", r.Active },
                    { "CompanyLogoURL", r.CompanyLogoURL }
                })) },
                { "CreatedAt", DateTime.UtcNow },
                { "IsDemo", true } // Flag to identify demo property
            };

            await propertiesCollection.InsertOneAsync(document);
            
            _logger.LogInformation("Demo property created successfully with ID {PropertyId}", DEMO_PROPERTY_ID);
            
            // Create sample bookings and menus
            await CreateDemoBookingsAsync();
            await CreateDemoMenusAsync();
            
            return DEMO_PROPERTY_ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create demo property");
            throw;
        }
    }

    /// <summary>
    /// Grants a user access to the demo property
    /// </summary>
    public async Task<bool> GrantUserAccessToDemoPropertyAsync(int userId)
    {
        try
        {
            // Ensure demo property exists first
            await EnsureDemoPropertyExistsAsync();

            var usersCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Users");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", userId);
            
            var user = await usersCollection.Find(filter).FirstOrDefaultAsync();
            
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when granting demo property access", userId);
                return false;
            }

            // Check if user already has access
            var propertyAccessList = user.Contains("PropertyAccessList") 
                ? user["PropertyAccessList"].AsBsonArray 
                : new MongoDB.Bson.BsonArray();

            var alreadyHasAccess = propertyAccessList.Any(p => 
                p.AsBsonDocument.Contains("PropertyID") && 
                p.AsBsonDocument["PropertyID"].AsInt32 == DEMO_PROPERTY_ID);

            if (alreadyHasAccess)
            {
                _logger.LogInformation("User {UserId} already has access to demo property", userId);
                return true;
            }

            // Add demo property access
            var demoAccess = new MongoDB.Bson.BsonDocument
            {
                { "PropertyID", DEMO_PROPERTY_ID },
                { "IsActive", true },
                { "From", DateTime.UtcNow },
                { "To", DateTime.UtcNow.AddYears(1) }, // 1 year access
                { "CreatedDate", DateTime.UtcNow },
                { "CreatedBy", userId },
                { "IsDemo", true }, // Flag to identify demo access
                { "Permissions", new MongoDB.Bson.BsonArray { "ViewOnly", "CanExplore" } } // Limited permissions
            };

            propertyAccessList.Add(demoAccess);

            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update
                .Set("PropertyAccessList", propertyAccessList);

            await usersCollection.UpdateOneAsync(filter, update);
            
            _logger.LogInformation("Granted user {UserId} access to demo property {PropertyId}", userId, DEMO_PROPERTY_ID);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant user {UserId} access to demo property", userId);
            return false;
        }
    }

    #region Private Helper Methods

    private Property CreateDemoProperty()
    {
        var demoRooms = new List<Property.Room>
        {
            new Property.Room("101", "Deluxe King Room", true, "room-101", "https://placehold.co/400x300/2196F3/white?text=Room+101"),
            new Property.Room("102", "Deluxe Queen Room", true, "room-102", "https://placehold.co/400x300/2196F3/white?text=Room+102"),
            new Property.Room("201", "Executive Suite", true, "room-201", "https://placehold.co/400x300/9C27B0/white?text=Suite+201"),
            new Property.Room("202", "Presidential Suite", true, "room-202", "https://placehold.co/400x300/9C27B0/white?text=Suite+202"),
            new Property.Room("301", "Family Room", true, "room-301", "https://placehold.co/400x300/FF9800/white?text=Family+301"),
            new Property.Room("302", "Ocean View Room", true, "room-302", "https://placehold.co/400x300/00BCD4/white?text=Ocean+302")
        };

        return new Property(DEMO_PROPERTY_NAME, true, demoRooms)
        {
            CompanyLogoURL = "https://placehold.co/200x200/4CAF50/white?text=DEMO",
            PropertyCode = DEMO_PROPERTY_CODE
        };
    }

    private async Task CreateDemoBookingsAsync()
    {
        var bookingsCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Bookings");
        
        // Check if demo bookings already exist
        var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("PropertyID", DEMO_PROPERTY_ID);
        var existingCount = await bookingsCollection.CountDocumentsAsync(filter);
        
        if (existingCount > 0)
        {
            _logger.LogInformation("Demo bookings already exist ({Count})", existingCount);
            return;
        }

        var demoBookings = new List<MongoDB.Bson.BsonDocument>
        {
            // Past booking
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -1 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "RoomCode", "101" },
                { "GuestName", "John Smith" },
                { "GuestEmail", "john.smith@example.com" },
                { "CheckIn", DateTime.UtcNow.AddDays(-10) },
                { "CheckOut", DateTime.UtcNow.AddDays(-7) },
                { "Status", "Completed" },
                { "TotalAmount", 450.00M },
                { "CreatedAt", DateTime.UtcNow.AddDays(-15) },
                { "IsDemo", true }
            },
            // Current booking
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -2 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "RoomCode", "201" },
                { "GuestName", "Sarah Johnson" },
                { "GuestEmail", "sarah.j@example.com" },
                { "CheckIn", DateTime.UtcNow.AddDays(-2) },
                { "CheckOut", DateTime.UtcNow.AddDays(2) },
                { "Status", "Active" },
                { "TotalAmount", 800.00M },
                { "CreatedAt", DateTime.UtcNow.AddDays(-5) },
                { "IsDemo", true }
            },
            // Future booking
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -3 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "RoomCode", "302" },
                { "GuestName", "Michael Brown" },
                { "GuestEmail", "m.brown@example.com" },
                { "CheckIn", DateTime.UtcNow.AddDays(5) },
                { "CheckOut", DateTime.UtcNow.AddDays(12) },
                { "Status", "Confirmed" },
                { "TotalAmount", 1200.00M },
                { "CreatedAt", DateTime.UtcNow.AddDays(-3) },
                { "IsDemo", true }
            },
            // Another future booking
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -4 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "RoomCode", "102" },
                { "GuestName", "Emma Davis" },
                { "GuestEmail", "emma.davis@example.com" },
                { "CheckIn", DateTime.UtcNow.AddDays(10) },
                { "CheckOut", DateTime.UtcNow.AddDays(14) },
                { "Status", "Pending" },
                { "TotalAmount", 600.00M },
                { "CreatedAt", DateTime.UtcNow },
                { "IsDemo", true }
            }
        };

        await bookingsCollection.InsertManyAsync(demoBookings);
        _logger.LogInformation("Created {Count} demo bookings", demoBookings.Count);
    }

    private async Task CreateDemoMenusAsync()
    {
        var menusCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Menus");
        
        // Check if demo menus already exist
        var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("PropertyID", DEMO_PROPERTY_ID);
        var existingCount = await menusCollection.CountDocumentsAsync(filter);
        
        if (existingCount > 0)
        {
            _logger.LogInformation("Demo menus already exist ({Count})", existingCount);
            return;
        }

        var demoMenus = new List<MongoDB.Bson.BsonDocument>
        {
            // Breakfast Menu
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -1 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "MenuName", "Breakfast Menu" },
                { "MenuType", "Breakfast" },
                { "Active", true },
                { "AvailableFrom", "06:00" },
                { "AvailableTo", "11:00" },
                { "Items", new MongoDB.Bson.BsonArray
                {
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Continental Breakfast" },
                        { "Description", "Croissant, jam, butter, coffee/tea" },
                        { "Price", 12.99M },
                        { "Category", "Breakfast" },
                        { "Available", true }
                    },
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Full English Breakfast" },
                        { "Description", "Eggs, bacon, sausage, beans, toast" },
                        { "Price", 18.99M },
                        { "Category", "Breakfast" },
                        { "Available", true }
                    },
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Pancake Stack" },
                        { "Description", "Stack of 3 with maple syrup" },
                        { "Price", 14.99M },
                        { "Category", "Breakfast" },
                        { "Available", true }
                    }
                }},
                { "CreatedAt", DateTime.UtcNow },
                { "IsDemo", true }
            },
            // Lunch Menu
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -2 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "MenuName", "Lunch Menu" },
                { "MenuType", "Lunch" },
                { "Active", true },
                { "AvailableFrom", "12:00" },
                { "AvailableTo", "15:00" },
                { "Items", new MongoDB.Bson.BsonArray
                {
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Caesar Salad" },
                        { "Description", "Romaine lettuce, parmesan, croutons" },
                        { "Price", 16.99M },
                        { "Category", "Salads" },
                        { "Available", true }
                    },
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Grilled Chicken Sandwich" },
                        { "Description", "With fries and coleslaw" },
                        { "Price", 19.99M },
                        { "Category", "Sandwiches" },
                        { "Available", true }
                    },
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Fish & Chips" },
                        { "Description", "Battered cod with chunky chips" },
                        { "Price", 22.99M },
                        { "Category", "Mains" },
                        { "Available", true }
                    }
                }},
                { "CreatedAt", DateTime.UtcNow },
                { "IsDemo", true }
            },
            // Dinner Menu
            new MongoDB.Bson.BsonDocument
            {
                { "_id", -3 },
                { "PropertyID", DEMO_PROPERTY_ID },
                { "MenuName", "Dinner Menu" },
                { "MenuType", "Dinner" },
                { "Active", true },
                { "AvailableFrom", "18:00" },
                { "AvailableTo", "22:00" },
                { "Items", new MongoDB.Bson.BsonArray
                {
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Ribeye Steak" },
                        { "Description", "12oz ribeye with vegetables" },
                        { "Price", 34.99M },
                        { "Category", "Steaks" },
                        { "Available", true }
                    },
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Grilled Salmon" },
                        { "Description", "Atlantic salmon with lemon butter" },
                        { "Price", 28.99M },
                        { "Category", "Seafood" },
                        { "Available", true }
                    },
                    new MongoDB.Bson.BsonDocument
                    {
                        { "ItemName", "Vegetarian Pasta" },
                        { "Description", "Penne with roasted vegetables" },
                        { "Price", 22.99M },
                        { "Category", "Pasta" },
                        { "Available", true }
                    }
                }},
                { "CreatedAt", DateTime.UtcNow },
                { "IsDemo", true }
            }
        };

        await menusCollection.InsertManyAsync(demoMenus);
        _logger.LogInformation("Created {Count} demo menus with items", demoMenus.Count);
    }

    #endregion
}
