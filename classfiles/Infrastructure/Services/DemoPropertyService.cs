using MongoDB.Driver;
using MyWarehouse.Domain.Property;
using Microsoft.Extensions.Logging;

namespace MyWarehouse.Infrastructure.Services;

/// <summary>
/// Service to manage the shared demo property for new users
/// Only assigns demo property if user doesn't have a valid property code
/// </summary>
public interface IDemoPropertyService
{
    Task<int> EnsureDemoPropertyExistsAsync();
    Task<bool> ShouldAssignDemoPropertyAsync(string? propertyCode);
    Task<int?> ValidateAndGetPropertyIdAsync(string propertyCode);
    Task<bool> GrantUserAccessToDemoPropertyAsync(int userId);
    Task<bool> GrantUserAccessToPropertyAsync(int userId, int propertyId);
}

public class DemoPropertyService : IDemoPropertyService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<DemoPropertyService> _logger;
    private readonly DemoPropertySeeder _seeder;
    private const int DEMO_PROPERTY_ID = -1;

    public DemoPropertyService(
        IMongoDatabase mongoDatabase,
        ILogger<DemoPropertyService> logger,
        DemoPropertySeeder seeder)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
        _seeder = seeder;
    }

    /// <summary>
    /// Determines if user should be assigned to demo property
    /// Returns true if propertyCode is null/empty/invalid
    /// </summary>
    public async Task<bool> ShouldAssignDemoPropertyAsync(string? propertyCode)
    {
        // If no property code provided, assign demo
        if (string.IsNullOrWhiteSpace(propertyCode))
        {
            _logger.LogInformation("No property code provided, will assign demo property");
            return true;
        }

        // Check if property code is valid
        var propertyId = await ValidateAndGetPropertyIdAsync(propertyCode);
        
        if (propertyId == null)
        {
            _logger.LogInformation("Invalid property code '{PropertyCode}', will assign demo property", propertyCode);
            return true;
        }

        _logger.LogInformation("Valid property code '{PropertyCode}' found (ID: {PropertyId}), will NOT assign demo property", 
            propertyCode, propertyId);
        return false;
    }

    /// <summary>
    /// Validates property code and returns property ID if valid
    /// Returns null if property code is invalid
    /// </summary>
    public async Task<int?> ValidateAndGetPropertyIdAsync(string propertyCode)
    {
        try
        {
            var propertiesCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Properties");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.And(
                MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("PropertyCode", propertyCode),
                MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("Active", true),
                MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Gte("_id", 0) // Only user properties, not demo
            );

            var property = await propertiesCollection.Find(filter).FirstOrDefaultAsync();
            
            if (property == null)
            {
                _logger.LogWarning("Property code '{PropertyCode}' not found or inactive", propertyCode);
                return null;
            }

            var propertyId = property["_id"].AsInt32;
            _logger.LogInformation("Property code '{PropertyCode}' validated successfully (ID: {PropertyId})", propertyCode, propertyId);
            return propertyId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating property code '{PropertyCode}'", propertyCode);
            return null;
        }
    }

    /// <summary>
    /// Ensures the demo property exists in the database
    /// Uses seeder to create from database template
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

            // Seed demo property from database template
            await _seeder.SeedDemoPropertyIfNotExistsAsync();
            
            return DEMO_PROPERTY_ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure demo property exists");
            throw;
        }
    }

    /// <summary>
    /// Grants a user access to the demo property
    /// Should only be called if user doesn't have a valid property code
    /// </summary>
    public async Task<bool> GrantUserAccessToDemoPropertyAsync(int userId)
    {
        return await GrantUserAccessToPropertyAsync(userId, DEMO_PROPERTY_ID);
    }

    /// <summary>
    /// Grants a user access to a specific property
    /// </summary>
    public async Task<bool> GrantUserAccessToPropertyAsync(int userId, int propertyId)
    {
        try
        {
            // If granting demo access, ensure demo property exists first
            if (propertyId == DEMO_PROPERTY_ID)
            {
                await EnsureDemoPropertyExistsAsync();
            }

            var usersCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Users");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", userId);
            
            var user = await usersCollection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when granting property access", userId);
                return false;
            }

            // Check if user already has access to this property
            // Handle both missing field and null value
            MongoDB.Bson.BsonArray propertyAccessList;
            if (user.Contains("PropertyAccessList") && user["PropertyAccessList"].IsBsonArray)
            {
                propertyAccessList = user["PropertyAccessList"].AsBsonArray;
            }
            else
            {
                propertyAccessList = new MongoDB.Bson.BsonArray();
            }

            var alreadyHasAccess = propertyAccessList.Any(p => 
                p.AsBsonDocument.Contains("Id") && 
                p.AsBsonDocument["Id"].AsInt32 == propertyId);

            if (alreadyHasAccess)
            {
                _logger.LogInformation("User {UserId} already has access to property {PropertyId}", userId, propertyId);
                return true;
            }

            // Add property access matching PropertyAccessList model
            var propertyAccess = new MongoDB.Bson.BsonDocument
            {
                { "Id", propertyId },  // Matches PropertyAccessList.Id field
                { "IsActive", true },
                { "From", DateTime.UtcNow },
                { "To", propertyId == DEMO_PROPERTY_ID ? DateTime.UtcNow.AddYears(1) : DateTime.MaxValue },
                { "CreatedDate", DateTime.UtcNow },
                { "CreatedBy", userId }
            };

            propertyAccessList.Add(propertyAccess);

            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update
                .Set("PropertyAccessList", propertyAccessList);

            await usersCollection.UpdateOneAsync(filter, update);

            _logger.LogInformation("Granted user {UserId} access to property {PropertyId}", userId, propertyId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant user {UserId} access to property {PropertyId}", userId, propertyId);
            return false;
        }
    }
}
