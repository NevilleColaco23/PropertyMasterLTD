namespace MongoDBBackend
{
    /// <summary>
    /// Interface that encapsulates a database Transaction
    /// Implementation may be done using TransactionScope, or in any other ORM specific way
    /// </summary>
    public interface IUnitOfWorkMongo : IDisposable
    {
        /// <summary>
        /// Commits the database Transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollbacks the database Transaction
        /// </summary>
        void Rollback();
    }
}