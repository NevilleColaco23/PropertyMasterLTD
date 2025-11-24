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

        // 4. Merge menu fields into root, prioritizing menu fields over original permission fields
        // This ensures 'label', 'path', 'order', 'hasDropdown', 'subItems', 'isVisible'
        // from the 'menu' document take precedence.
        new BsonDocument(MongoStages.REPLACEROOT, new BsonDocument
        {
            { "newRoot", new BsonDocument(MongoStages.MERGEOBJECTS, new BsonArray { "$$ROOT", "$menu" }) }
        }),

        // 5. Explicitly project desired fields, ensuring subItems is included.
        // This makes the output schema clear and guarantees subItems' presence.
        new BsonDocument(MongoStages.PROJECT, new BsonDocument
        {
            { "_id", "$_id" }, // The _id from the original permission document
            { "userId", "$userId" },
            { "isActive", "$isActive" },
            { "accessLevel", "$accessLevel" },
            { "menuId", "$menuId" }, // The menuId from the original permission document

            // Fields from the 'menu' document (now at the root level, prioritized by mergeObjects)
            { "label", "$label" },
            { "path", "$path" },
            { "order", "$order" },
            { "hasDropdown", "$hasDropdown" },
            { "subItems", "$subItems" }, // Explicitly include subItems
            { "isVisible", "$isVisible" },
            { "menuCreatedAt", "$createdAt" }, // Renamed to avoid conflict if original doc also has createdAt
            { "menuUpdatedAt", "$updatedAt" }  // Renamed to avoid conflict if original doc also has updatedAt
        }),

        // 6. Filter only visible menus (using the isVisible from the menu document)
        new BsonDocument(MongoStages.MATCH, new BsonDocument("isVisible", true)),

        // 7. Lookup Property documents whose AccessList contains this userId
        //    and that are Active.
        new BsonDocument(MongoStages.LOOKUP, new BsonDocument
        {
            { "from", "Property" },
            { "let", new BsonDocument("uid", "$userId") },
            { "pipeline", new BsonArray
                {
                    new BsonDocument(MongoStages.MATCH,
                        new BsonDocument("$expr",
                            new BsonDocument("$in",
                                new BsonArray { "$$uid", "$AccessList" }
                            )
                        )
                    ),
                    new BsonDocument(MongoStages.MATCH, new BsonDocument("Active", true)),
                    new BsonDocument(MongoStages.PROJECT, new BsonDocument
                    {
                        { "_id", 1 },
                        { "Name", 1 },
                        { "Rooms", 1 },
                        { "CompanyLogoURL", 1 },
                        { "AccessList", 1 },
                        { "Active", 1 }
                    })
                }
            },
            { "as", "property" }
        }),

        // 9. Convert property array to a single object (or null).
        new BsonDocument(MongoStages.ADDFIELDS, new BsonDocument
        {
            { "property", new BsonDocument("$arrayElemAt", new BsonArray { "$property", 0 }) }
        }),

        // 10. Sort by order (using the order from the menu document)
        new BsonDocument(MongoStages.SORT, new BsonDocument("order", 1)),

        
    };
        }


        public string QueryStr =>
        $"[{{ $match: {{ }} }}]";

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;
    }
}

//db.getCollection("MenuPermissions").aggregate([
//  {
//    "$match": {
//        "userId": 1,
//      "isActive": true,
//      "accessLevel": { "$ne": "hidden" }
//    }
//},

//  {
//    "$lookup": {
//        "from": "Menus",
//      "localField": "menuId",
//      "foreignField": "_id",
//      "as": "menu"
//    }
//},

//  { "$unwind": "$menu" },

//  {
//    "$replaceRoot": {
//        "newRoot": { "$mergeObjects": ["$$ROOT", "$menu"] }
//    }
//},

//  {
//    "$project": {
//        "_id": "$_id",
//      "userId": "$userId",
//      "isActive": "$isActive",
//      "accessLevel": "$accessLevel",
//      "menuId": "$menuId",
//      "label": "$label",
//      "path": "$path",
//      "order": "$order",
//      "hasDropdown": "$hasDropdown",
//      "subItems": "$subItems",
//      "isVisible": "$isVisible",
//      "menuCreatedAt": "$createdAt",
//      "menuUpdatedAt": "$updatedAt"
//    }
//},

//  { "$match": { "isVisible": true } },

//  // Lookup Property documents whose AccessList contains the current userId
//  {
//    "$lookup": {
//        "from": "Property",
//      "let": { "uid": "$userId" },
//      "pipeline": [
//        { "$match": { "$expr": { "$in": ["$$uid", "$AccessList"] } } },
//        { "$match": { "Active": true } }, 
//        { "$project": { "_id": 1, "Name": 1, "Rooms": 1, "CompanyLogoURL": 1, "AccessList": 1, "Active": 1 } }
//      ],
//      "as": "property"
//    }
//},

//  // Convert the property array to a single object (first match) or null
//  { "$addFields": { "property": { "$arrayElemAt": ["$property", 0] } } },

//  { "$sort": { "order": 1 } }
//])