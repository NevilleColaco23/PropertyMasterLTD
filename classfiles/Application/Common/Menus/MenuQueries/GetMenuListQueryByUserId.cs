using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Common.Menus.MenuQueries
{
    internal class GetMenuListQueryByUserId : INamedQuery
    {
        private readonly int _userId;
        private readonly string _filterString;

        public GetMenuListQueryByUserId(int userId)
        {
            _userId = userId;
        }

        public BsonArray? BsonPipeline => string.IsNullOrEmpty(_filterString) ? GetMenuItemsPipeline(_userId)
        : null;

        private BsonArray GetMenuItemsPipeline(int userId)
        {
            return new BsonArray
    {
        // 1. Match user-specific active permissions (excluding hidden)
        new BsonDocument(MongoStages.MATCH, new BsonDocument
        {
            { "userId", userId },
            { "isActive", true },
            { "accessLevel", new BsonDocument("$ne", "hidden") }
        }),

        // 2. Join with Menus collection
        new BsonDocument(MongoStages.LOOKUP, new BsonDocument
        {
            { "from", "Menus" },
            { "localField", "menuId" },
            { "foreignField", "_id" },
            { "as", "menu" }
        }),

        // 3. Flatten the menu array
        new BsonDocument(MongoStages.UNWIND, "$menu"),

        // 4. Merge menu fields into root
        new BsonDocument(MongoStages.REPLACEROOT, new BsonDocument
        {
            { "newRoot", new BsonDocument(MongoStages.MERGEOBJECTS, new BsonArray { "$menu", "$$ROOT" }) }
        }),

        // 5. Remove the now-redundant menu field
        new BsonDocument(MongoStages.PROJECT, new BsonDocument("menu", 0)),

        // 6. Filter only visible menus
        new BsonDocument(MongoStages.MATCH, new BsonDocument("isVisible", true)),

        // 7. Sort by order
        new BsonDocument(MongoStages.SORT, new BsonDocument("order", 1))
    };
        }


        public string QueryStr =>
        $"[{{ $match: {{ }} }}]";

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;
    }
}