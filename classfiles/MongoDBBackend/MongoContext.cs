using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MyWarehouse.Domain.Common;

namespace MongoDBBackend
{
    class MongoContext : IPersistenceContextMongo
    {
        private readonly IClientSessionHandle _session;
        private readonly string _dbName;
        private bool _isDisposed;

        internal MongoContext(IClientSessionHandle session, string dbName)
        {
            _session = session;
            _dbName = dbName;
        }

        public IMongoRepository GetMongoRepository()
        {
            return new MongoRepository(this);
        }

        public IRepositoryMongo<T> GetRepository<T>(string tableName = null) where T : EntityBase
        {
            return new MongoEntityRepository<T>(this, tableName);
        }

        public IUnitOfWorkMongo StartUnitOfWork()
        {
            return new MongoUnitOfWork(_session);
        }

        public void CreateView(string viewName, string viewOn, INamedQueryMongo pipelineQuery)
        {
            var stages = BsonSerializer.Deserialize<BsonArray>(pipelineQuery?.QueryStr ?? "[ {\"$match\": { }} ]").Select(p => p.AsBsonDocument).ToList();    // Add Empty Filter if no Query String is given
            DropView(viewName);
            GetDatabase().CreateView(viewName, viewOn, PipelineDefinition<BsonDocument, BsonDocument>.Create(stages));
        }

        public void DropView(string viewName)
        {
            GetDatabase().DropCollection(viewName);
        }

        public string GetDatabaseName()
        {
            return _dbName;
        }


        // Internal Methods

        internal IMongoCollection<T> GetCollection<T>(string name = null)
        {
            return _session.Client.GetDatabase(_dbName).GetCollection<T>(name ?? GetTableName<T>());
        }

        internal IMongoDatabase GetDatabase()
        {
            return _session.Client.GetDatabase(_dbName);
        }

        private static string GetTableName<T>()
        {
            var descriptions = (TableAttribute[])typeof(T).GetCustomAttributes(typeof(TableAttribute), false);

            return descriptions.Length == 0 ? typeof(T).Name : descriptions[0].Name;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _session.Dispose();

            _isDisposed = true;
        }

        public async Task<int> GetNextIncrementedValue<T>(string collectionName = null) where T : EntityBase
        {
            collectionName = string.IsNullOrEmpty(collectionName) ? "Customer" : collectionName;
            var collection = _session.Client.GetDatabase(_dbName).GetCollection<BsonDocument>("Customer");//collectionName ?? GetTableName<T>()

            try
            {
                // Creating a filter to find the document with the specified collection name
                var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);

                // Creating an update definition to increment the "seq" field by 1
                var update = Builders<BsonDocument>.Update.Inc("seq", 1);

                // Setting options for the FindOneAndUpdate operation
                var options = new FindOneAndUpdateOptions<BsonDocument>
                {
                    IsUpsert = true, // If no document matches the filter, a new one will be created
                    ReturnDocument = ReturnDocument.After // Returns the updated document
                };

                // Finding and updating the document with the provided filter and update
                var result = await collection.FindOneAndUpdateAsync(filter, update, options);

                // Returning the value of the "seq" field from the updated document
                return result["seq"].AsInt32;
            }
            catch (Exception ex)
            {
                // Handling the exception as needed
                throw;
            }

        }


        #region Code with mongo transaction #TODO: enalbing mongo transaction using replica set
        //public async Task<int> GetNextIncrementedValue<T>(string collectionName = null) where T : EntityBase
        //{
        //    var collection = _session.Client.GetDatabase(_dbName).GetCollection<BsonDocument>("Customer"); //collectionName ?? GetTableName<T>()

        //    // Starting a session to group multiple operations as a transaction
        //    var session = await _session.Client.StartSessionAsync();
        //    try
        //    {
        //        // Starting a transaction for the session
        //        session.StartTransaction();

        //        // Creating a filter to find the document with the specified collection name
        //        var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);

        //        // Creating an update definition to increment the "seq" field by 1
        //        var update = Builders<BsonDocument>.Update.Inc("seq", 1);

        //        // Setting options for the FindOneAndUpdate operation
        //        var options = new FindOneAndUpdateOptions<BsonDocument>
        //        {
        //            IsUpsert = true, // If no document matches the filter, a new one will be created
        //            ReturnDocument = ReturnDocument.After // Returns the updated document
        //        };

        //        // Finding and updating the document with the provided filter and update
        //        var result = await collection.FindOneAndUpdateAsync(session, filter, update, options);

        //        // Committing the transaction if all the operations are successful
        //        await session.CommitTransactionAsync();

        //        // Returning the value of the "seq" field from the updated document
        //        return result["seq"].AsInt32;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Aborting the transaction if an error occurs
        //        await session.AbortTransactionAsync();

        //        // Rethrowing the exception to be handled by the calling code
        //        throw;
        //    }
        //    finally
        //    {
        //        // Disposing of the session to release associated resources
        //        session.Dispose();
        //    }
        //}
        #endregion
    }
}
