using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Domain.Property;
using System.Data;

namespace MyWarehouse.Application.Common.Bookings.BookingsQuery
{
    public class GetBookingsMongoQuery : INamedQuery
    {
        private readonly string _bookingId;
        private readonly string _filterString;
        private readonly int _pageIndex;
        private readonly int _pageSize;
        private readonly string _orderBy;
        private readonly int _sortDirection;
        private readonly bool _getAllProperties;

        public GetBookingsMongoQuery(string bookingId, string filterString, int pageIndex, int pageSize, string orderBy, int sortDirection
            ,bool getAllProperties = false)
        {
            _bookingId = bookingId;
            _filterString = filterString;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
            _orderBy = orderBy;
            _sortDirection = sortDirection;
            _getAllProperties = getAllProperties;
        }

        public BsonArray? BsonPipeline => GetBookingsPipeline();

        private BsonArray GetBookingsPipeline()
        {
            var pipeline = new BsonArray();

            // Build a single match document with optional conditions
            var matchDoc = new BsonDocument();

            // Conditionally apply propertyId filter
            if (_getAllProperties)
            {
                matchDoc.Add("propertyId", new BsonDocument("$in", new BsonArray { 1, 1 })); //TODO: Adjust this as needed for "all properties" logic
            }

            // Conditionally apply text/number search filter
            if (!string.IsNullOrWhiteSpace(_filterString))
            {
                var orConditions = new BsonArray
        {
            new BsonDocument("bookingId",
                new BsonDocument("$regex", new BsonRegularExpression(_filterString, "i"))),
            new BsonDocument("roomNumber",
                new BsonDocument("$regex", new BsonRegularExpression(_filterString, "i"))),
        };

                if (long.TryParse(_filterString, out var guestIdToSearch))
                {
                    orConditions.Add(new BsonDocument("guestId", guestIdToSearch));
                }

                matchDoc.Add("$or", orConditions);
            }

            // Only add $match if we actually have something to match on
            if (matchDoc.ElementCount > 0)
            {
                pipeline.Add(new BsonDocument("$match", matchDoc));
            }

            pipeline.Add(new BsonDocument("$facet", new BsonDocument
    {
        { "results", new BsonArray
            {
                new BsonDocument("$sort", new BsonDocument(_orderBy, _sortDirection)),
                new BsonDocument("$skip", ((_pageIndex - 1) * _pageSize)),
                new BsonDocument("$limit", _pageSize)
            }
        },
        { "totalCount", new BsonArray
            {
                new BsonDocument("$count", "count")
            }
        }
    }));

            return pipeline;
        }

        public string QueryStr => string.Empty;

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;
    }
}
