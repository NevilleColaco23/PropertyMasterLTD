using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MyWarehouse.Domain.Common;

namespace MongoDBBackend
{
    public class MongoContextFactory : IPersistenceContextFactory
    {
        private readonly MongoClient _client;
        private readonly string _dbName;

        public MongoContextFactory(string connectionString, IEnumerable<Assembly> assemblies)
        {
            var mongoUrl = new MongoUrl(connectionString);
            _dbName = mongoUrl.DatabaseName;
            _client = new MongoClient(mongoUrl);

            foreach (Assembly assembly in assemblies)
            {
                RegisterMappings(GetMappingsFromAssembly(assembly));
            }

            var pack = new ConventionPack();
            //pack.AddClassMapConvention("IdMap", map => map.IdMemberMap.SetElementName("Id"));
            ConventionRegistry.Register("IdMapConvention", pack, t => t.IsSubclassOf(typeof(EntityBase)));//EntityBase

            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
        }

        public IPersistenceContext CreateContext(string dbName = null)
        {
            return new MongoContext(_client.StartSession(), dbName ?? _dbName);
        }

        private IEnumerable<Type> GetMappingsFromAssembly(Assembly assembly)
        {
            // Returns all classes derived from BsonClassMap & which are not abstract.
            return assembly.GetTypes().Where(t => t.Name == "MongoClassMap");
        }

        private void RegisterMappings(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                Activator.CreateInstance(type);
            }
        }
    }
}
