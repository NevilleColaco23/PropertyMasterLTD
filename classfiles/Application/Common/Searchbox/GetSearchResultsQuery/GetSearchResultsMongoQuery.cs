using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Common.Searchbox.GetSearchResultsMongoQuery;

public class GetSearchResultsMongoQuery : INamedQuery
{
    private readonly string _searchText;
    private readonly string _filterString;

    public GetSearchResultsMongoQuery(string searchText, string filterString)
    {
        _filterString = filterString;
        _searchText = searchText;
    }

    public BsonArray? BsonPipeline => string.IsNullOrEmpty(_filterString) ? GetSearchSuggestionPipeline(_searchText)
        : null;


    private BsonArray GetSearchSuggestionPipeline(string query)
    {
        // Your existing pipeline up to the problematic point
        var pipeline = new BsonArray
    {
        new BsonDocument("$match", new BsonDocument("isVisible", true)),
        new BsonDocument("$project", new BsonDocument
        {
            { "matches", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$subItems" },
                    { "as", "item" },
                    { "cond", new BsonDocument("$regexMatch", new BsonDocument
                        {
                            { "input", "$$item.subLabel" },
                            { "regex", query },
                            { "options", "i" }
                        })
                    }
                })
            },
            { "label", 1 }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "suggestions", new BsonDocument("$concatArrays", new BsonArray {
                new BsonArray { "$label" },
                new BsonDocument("$map", new BsonDocument
                {
                    { "input", "$matches" },
                    { "as", "m" },
                    { "in", "$$m.subLabel" }
                })
            })}
        }),
        new BsonDocument("$unwind", "$suggestions"),
        new BsonDocument("$match", new BsonDocument("suggestions", new BsonDocument("$regex", query).Add("$options", "i"))),
        // --- Add these for debugging ---
        //new BsonDocument("$limit", 10), // Limit to a few documents for easier inspection
        //new BsonDocument("$out", "temp_debug_collection"), // Output to a temporary collection
        // -----------------------------
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", 1 },
            { "searchedItem", new BsonDocument("$addToSet", "$suggestions") }
        })
    };
        return pipeline;
    }

    public CommandType CommandType => CommandType.Text;

    public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;

    public string QueryStr => throw new NotImplementedException();

}