using MongoDB.Driver;

namespace MongoDBBackend
{
    class MongoUnitOfWork : IUnitOfWorkMongo
    {
        //private readonly IClientSessionHandle _session;

        public MongoUnitOfWork(IClientSessionHandle session)
        {
            //_session = session;
            //_session.StartTransaction();        // Not supported currently in MongoDb
        }

        public void Commit()
        {
            //_session.CommitTransaction();       // Not supported currently in MongoDb
        }

        public void Rollback()
        {
            //_session.AbortTransaction();        // Not supported currently in MongoDb
        }

        public void Dispose()
        {
            //if (_session.IsInTransaction)        // Not supported currently in MongoDb
            //    _session.AbortTransaction();
        }
    }
}
