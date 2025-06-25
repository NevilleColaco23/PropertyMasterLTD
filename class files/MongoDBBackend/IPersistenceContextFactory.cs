using MongoDBBackend;

namespace MongoDBBackend
{
    /// <summary>
    /// Interface to be implemented by the database connection manager
    /// Implementation may use the default connection pooling provided by Dot-Net or any other ORM specific implementation
    /// </summary>
    public interface IPersistenceContextFactory
    {
        /// <summary>
        /// Creates a database session
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns>IPersistenceContext - A wrapper over the database session</returns>
        IPersistenceContext CreateContext(string dbName = null);
    }
}