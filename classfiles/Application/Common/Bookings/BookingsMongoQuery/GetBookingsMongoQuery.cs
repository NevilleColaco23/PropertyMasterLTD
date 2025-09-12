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

        public GetBookingsMongoQuery(string bookingId, int pageIndex, int pageSize)
        {
            _bookingId = bookingId;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
        }

        public BsonArray? BsonPipeline => GetBookingsPipeline();

        private BsonArray GetBookingsPipeline()
        {
            return new BsonArray
                {
                    new BsonDocument("$facet", new BsonDocument
                    {
                        { "results", new BsonArray
                            {
                                // The $sort stage is correctly placed before $skip and $limit
                                new BsonDocument("$sort", new BsonDocument("lastModified", -1)),
                                new BsonDocument("$skip", (_pageIndex - 1) * _pageSize), //- 1 needed as had to increment by 1 in UI
                                new BsonDocument("$limit", _pageSize)
                            }
                        },
                        { "totalCount", new BsonArray
                            {
                                new BsonDocument("$count", "count")
                            }
                        }
                    })
                };
        }

        public string QueryStr => string.Empty;

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;

    }
}