using MongoDBBackend;
using MyWarehouse.Domain.Common;

namespace MongoDBBackend
{
    /// <summary>
    /// Interface that encapsulates a database connection
    /// </summary>
    public interface IPersistenceContext : IDisposable
    {
        /// <summary>
        /// Starts a new database transaction.
        /// </summary>
        /// <returns>IUnitOfWorkMongo - A wrapper over the database transaction</returns>
        IUnitOfWorkMongo StartUnitOfWork();

        IRepositoryMongo<T> GetRepository<T>(string tableName = null) where T : EntityBase;
    }

    public interface IPersistenceContextSql : IPersistenceContext
    {
        INamedQueryRepositoryMongo CreateNamedQueryRepository();

    }

    public interface IPersistenceContextMongo : IPersistenceContext
    {
        IMongoRepository GetMongoRepository();

        void CreateView(string viewName, string viewOn, INamedQueryMongo pipelineQuery);

        void DropView(string viewName);

        string GetDatabaseName();
    }
}
