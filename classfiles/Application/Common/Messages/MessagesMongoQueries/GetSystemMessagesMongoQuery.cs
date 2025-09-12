using MongoDB.Bson;
using MongoDBBackend;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Common.Messages.MessagesQueries
{
    public class GetSystemMessagesMongoQuery : INamedQuery
    {
        private readonly string _filterString;

        public GetSystemMessagesMongoQuery()
        {
        }

        public BsonArray? BsonPipeline => string.IsNullOrEmpty(_filterString) ? GetSystemMessageQueuePipeline()
        : null;

        private BsonArray GetSystemMessageQueuePipeline()
        {
            return new BsonArray
            {
            new BsonDocument(MongoStages.SORT, new BsonDocument("order", 1))
            };
        }


        public string QueryStr =>
        $"[{{ $match: {{ }} }}]";

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<Common.Dependencies.DataAccess.NamedQueryParameter> Parameters => null;
    }
}