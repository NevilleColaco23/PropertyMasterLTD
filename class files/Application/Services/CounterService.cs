using MongoDB.Driver;
using MyWarehouse.Domain.Other_Entities;

namespace MyWarehouse.Application.Services
{
    public class CounterService : ICounterService
    {
        private readonly IMongoCollection<KeyCounter> _counterCollection;

        public CounterService(IMongoDatabase database)
        {
            _counterCollection = database.GetCollection<KeyCounter>(MongoCollections.KeyCounterCollection);
        }

        public async Task<int> GetNextSequenceValue(string entityName)
        {
            var filter = Builders<KeyCounter>.Filter.Eq(c => c.EntityName, entityName);
            var update = Builders<KeyCounter>.Update.Inc(c => c.Sequence, 1);
            var options = new FindOneAndUpdateOptions<KeyCounter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var counter = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            return counter.Sequence;
        }
    }
}
