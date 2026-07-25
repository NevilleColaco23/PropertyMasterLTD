using System.Data;
using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Property.PropertyQueries;

/// <summary>
/// Returns ONLY properties the user has access to based on User.PropertyAccessList
/// Used by: Property Selector (user selecting which property to work with)
/// </summary>
public class GetPropertyQueryByUserIdUsingMongoQueryString : INamedQuery
{
    private readonly int _userId;
    private readonly string _filterString;

    public GetPropertyQueryByUserIdUsingMongoQueryString(int userId, string filterString)
    {
        _filterString = filterString;
        _userId = userId;
    }

    public BsonArray? BsonPipeline => string.IsNullOrEmpty(_filterString) ? GetUserAccessiblePropertiesPipeline(_userId)
        : null;

    private BsonArray GetUserAccessiblePropertiesPipeline(int userId)
    {
        return new BsonArray
        {
            // STEP 1: Lookup the User to get their PropertyAccessList
            new BsonDocument(MongoStages.LOOKUP, new BsonDocument
            {
                { MongoStages.FROM, "Users" },
                { MongoStages.LET, new BsonDocument("propertyId", "$_id") },
                { MongoStages.PIPELINE, new BsonArray
                    {
                        new BsonDocument(MongoStages.MATCH, new BsonDocument
                        {
                            { "_id", userId }
                        }),
                        new BsonDocument(MongoStages.PROJECT, new BsonDocument
                        {
                            { "hasAccess", new BsonDocument("$in", new BsonArray 
                                { 
                                    "$$propertyId", 
                                    new BsonDocument("$map", new BsonDocument
                                    {
                                        { "input", new BsonDocument("$ifNull", new BsonArray { "$PropertyAccessList", new BsonArray() }) },
                                        { "as", "pa" },
                                        { "in", "$$pa._id" }
                                    })
                                }) 
                            }
                        })
                    }
                },
                { MongoStages.AS, "userAccess" }
            }),

            // STEP 2: Unwind the userAccess array
            new BsonDocument(MongoStages.UNWIND, new BsonDocument
            {
                { "path", "$userAccess" },
                { "preserveNullAndEmptyArrays", false }
            }),

            // STEP 3: Match ONLY properties where user has access
            new BsonDocument(MongoStages.MATCH, new BsonDocument
            {
                { "userAccess.hasAccess", true }
            }),

            // STEP 4: Project fields
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
                { "UpdatedBy", 1 }
            })
        };
    }

    public CommandType CommandType => CommandType.Text;

    public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;

    public string QueryStr => throw new NotImplementedException();
}
