using System.Data;
using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Property.PropertyQueries;

/// <summary>
/// Returns ALL properties without any access control filtering
/// Used by: Property Master admin page
/// </summary>
public class GetAllPropertiesMongoQuery : INamedQuery
{
    public BsonArray? BsonPipeline => GetAllPropertiesPipeline();

    private BsonArray GetAllPropertiesPipeline()
    {
        return new BsonArray
        {
            // Return ALL properties including deleted ones for Property Master admin page
            // The frontend will handle filtering by status (active/inactive/deleted)
            new BsonDocument(MongoStages.PROJECT, new BsonDocument
            {
                { "_id", 1 },
                { "Id", "$_id" },
                { "Name", 1 },
                { "Active", 1 },
                { "PropertyCode", 1 },
                { "Rooms", new BsonDocument(MongoStages.FILTER, new BsonDocument
                    {
                        { "input", new BsonDocument("$ifNull", new BsonArray { "$Rooms", new BsonArray() }) },
                        { "as", "room" },
                        { "cond", new BsonDocument(MongoStages.EQ, new BsonArray { "$$room.Active", true }) }
                    })
                },
                { "CompanyLogoURL", 1 },
                { "CreatedAt", 1 },
                { "CreatedBy", 1 },
                { "UpdatedAt", 1 },
                { "UpdatedBy", 1 },
                { "IsDeleted", 1 },
                { "DeletedAt", 1 },
                { "DeletedBy", 1 },
                { "AccessList", 1 }
            })
        };
    }

    public CommandType CommandType => CommandType.Text;

    public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;

    public string QueryStr => throw new NotImplementedException();
}
