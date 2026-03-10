using System.Data;
using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Property.PropertyQueries;

public class GetPropertyQueryByUserIdUsingMongoQueryString : INamedQuery
{
    private readonly int _propertyId;
    private readonly string _filterString;

    public GetPropertyQueryByUserIdUsingMongoQueryString(int propertyid, string filterString)
    {
        _filterString = filterString;
        _propertyId = propertyid;
    }

    public BsonArray? BsonPipeline => string.IsNullOrEmpty(_filterString) ? GetPropertyListPipeline(_propertyId)
        : null;

    private BsonArray? GetPropertyListPipeline(int propertyId)
    {
        return new BsonArray
        {
            // STEP 1: Filter by current user ID
            new BsonDocument(MongoStages.MATCH, new BsonDocument
            {
                { "_id", propertyId }  // Match the current user
            }),

            // STEP 2: Lookup properties from Property collection
            new BsonDocument(MongoStages.LOOKUP, new BsonDocument
            {
            { MongoStages.FROM, "Property" },
            { MongoStages.LET, new BsonDocument("propertyIds", new BsonDocument("$map", new BsonDocument
                {
                    { "input", "$PropertyAccessList" },
                    { "as", "pa" },
                    { "in", "$$pa.Id" }  // Fixed: Changed from PropertyID to Id
                }))
            },
            { MongoStages.PIPELINE, new BsonArray
                {
                    // Filter properties by IDs in user's PropertyAccessList
                    new BsonDocument(MongoStages.MATCH, new BsonDocument("$expr", new BsonDocument("$in", new BsonArray { "$_id", "$$propertyIds" }))),
                    new BsonDocument(MongoStages.PROJECT, new BsonDocument
                    {
                        { "_id", 1 },
                        { "Name", 1 },
                        { "Rooms", new BsonDocument(MongoStages.FILTER, new BsonDocument
                            {
                                { "input", "$Rooms" },
                                { "as", "room" },
                                { "cond", new BsonDocument(MongoStages.EQ, new BsonArray { "$$room.Active", true }) }
                            })
                        },
                        { "CompanyLogoURL", 1 }
                    })
                }
            },
            { MongoStages.AS, "PropertyList" }
        }),
        new BsonDocument(MongoStages.UNWIND, "$PropertyList"),
        new BsonDocument(MongoStages.PROJECT, new BsonDocument
        {
            { "_id", 0 },
            { "Name", "$PropertyList.Name" },
            { "Id", "$PropertyList._id" },
            { "Rooms", "$PropertyList.Rooms" },
            { "CompanyLogoURL", "$PropertyList.CompanyLogoURL" }
        })

        };
    }

    public CommandType CommandType => CommandType.Text;

    public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;

    public string QueryStr => throw new NotImplementedException();


    // Uncomment the following if you want to use a string-based query instead of BsonArray
    //public string QueryStr => string.IsNullOrEmpty(_filterString) ?
    //$"[{{ $match: {{ _id: {_propertyId} }} }}" +
    //$"," +
    //$"{{ $lookup: {{ from: \"Property\", let: {{ propertyIds: {{ $map: {{ input: \"$PropertyAccessList\", as: \"pa\", in: \"$$pa.PropertyID\" }} }} }}, pipeline: [ {{ $match: {{ $expr: {{ $and: [ {{ $in: [\"$_id\", \"$$propertyIds\"] }}, {{ $eq: [\"$Active\", true] }} ] }} }} }}" +
    //$", {{ $project: {{ _id: 1, Name: 1, Rooms: {{ $filter: {{ input: \"$Rooms\", as: \"room\", cond: {{ $eq: [\"$$room.Active\", true] }} }} }} }} }} ], as: \"PropertyList\" }} }}, {{ $unwind: \"$PropertyList\" }}" +
    //$", {{ $project: {{ _id: 0, Name: \"$PropertyList.Name\", Id: \"$PropertyList._id\", Rooms: \"$PropertyList.Rooms\" }} }}" +
    //$"]"
    //:
    //$"[{{ \"$match\": {{ $expr: {_filterString} }} }}]";
}