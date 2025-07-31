using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Common.Bookings.BookingsQuery
{
    public class GetBookingsMongoQuery : INamedQuery
    {
        private readonly string _bookingid;
        private readonly string _filterString;

        public GetBookingsMongoQuery(string bookingId)
        {
            _bookingid = bookingId;
        }

        public BsonArray? BsonPipeline => string.IsNullOrEmpty(_filterString) ? GetBookingsPipeline(_bookingid)
        : null;

        private BsonArray GetBookingsPipeline(string bookingsId)
        {
            return new BsonArray
    {
        // 1. Match user-specific active permissions (excluding hidden)
        new BsonDocument(MongoStages.MATCH, new BsonDocument
        {
            { "userId", bookingsId },
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

        // 7. Sort by order (using the order from the menu document)
        new BsonDocument(MongoStages.SORT, new BsonDocument("order", 1))
    };
        }


        public string QueryStr =>
        $"[{{ $match: {{ }} }}]";

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;
    }
}