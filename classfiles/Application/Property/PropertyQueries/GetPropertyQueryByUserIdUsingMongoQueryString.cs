using System.Data;
using MongoDB.Bson;
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
            new BsonDocument("$match", new BsonDocument("_id", propertyId)),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Property" },
                { "let", new BsonDocument("propertyIds", new BsonDocument("$map", new BsonDocument
                    {
                        { "input", "$PropertyAccessList" },
                        { "as", "pa" },
                        { "in", "$$pa.PropertyID" }
                    }))
                },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                        {
                            new BsonDocument("$in", new BsonArray { "$_id", "$$propertyIds" }),
                            new BsonDocument("$eq", new BsonArray { "$Active", true })
                        }))),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 1 },
                            { "Name", 1 },
                            { "Rooms", new BsonDocument("$filter", new BsonDocument
                                {
                                    { "input", "$Rooms" },
                                    { "as", "room" },
                                    { "cond", new BsonDocument("$eq", new BsonArray { "$$room.Active", true }) }
                                })
                            }
                        })
                    }
                },
                { "as", "PropertyList" }
            }),
            new BsonDocument("$unwind", "$PropertyList"),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "Name", "$PropertyList.Name" },
                { "Id", "$PropertyList._id" },
                { "Rooms", "$PropertyList.Rooms" }
            })
        };
    }

    public CommandType CommandType => CommandType.Text;

    public IReadOnlyList<NamedQueryParameter> Parameters => null;

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