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
            return new BsonArray();
        }

        public string QueryStr =>
        $"[{{ $match: {{ }} }}]";

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;
    }
}