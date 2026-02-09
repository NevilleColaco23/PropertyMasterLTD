using MongoDB.Driver;
using testAngularAPI.Server.Model;

namespace testAngularAPI.Server.Mongo
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDb:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDb:DatabaseName"]);
        }

        public IMongoCollection<Property> Properties => _database.GetCollection<Property>("Property");

        public IMongoCollection<User> Users => _database.GetCollection<User>("User");
    }
}
