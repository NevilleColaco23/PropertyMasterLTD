using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
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

        public GetBookingsMongoQuery(string bookingId, string filterString, int pageIndex, int pageSize, string orderBy, int sortDirection)
        {
            _bookingId = bookingId;
            _filterString = filterString;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
            _orderBy = orderBy;
            _sortDirection = sortDirection;
        }

        public BsonArray? BsonPipeline => GetBookingsPipeline();

        private BsonArray GetBookingsPipeline()
        {
            var pipeline = new BsonArray();

            if (!string.IsNullOrWhiteSpace(_filterString))
            {
                var orConditions = new BsonArray
        {
            // Case-insensitive search on string fields
            new BsonDocument("bookingId", new BsonDocument("$regex", new BsonRegularExpression(_filterString, "i"))),
            new BsonDocument("roomNumber", new BsonDocument("$regex", new BsonRegularExpression(_filterString, "i"))),
        };

                // Attempt to parse the filter string as a long for numeric fields
                if (long.TryParse(_filterString, out long guestIdToSearch))
                {
                    orConditions.Add(new BsonDocument("guestId", guestIdToSearch));
                }

                var matchStage = new BsonDocument("$match", new BsonDocument("$or", orConditions));
                pipeline.Add(matchStage);
            }

            // Always add the $facet stage
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
