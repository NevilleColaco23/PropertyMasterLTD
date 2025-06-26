using System.Linq.Expressions;
using MyWarehouse.Domain.Common;

namespace MongoDBBackend
{
    //TODO : Check if can be added in application project under RepositoryMongo folder and reference it into domain project from there
    /// <summary>
    /// Encapsulates the selection criteria for entities.
    /// </summary>
    /// <typeparam name="T">The type of entity on which the selection criteria applies</typeparam>
    public interface ISpecificationGeneric<T>
    {
        /// <summary>
        /// Encapsulates the selection criteria for entities.
        /// </summary>
        /// <returns>The Linq expression to be used for entity selection</returns>
        Expression<Func<T, bool>> IsSatisfiedBy();

        /// <summary>
        /// Flag to tell the ORM if the results of the query should be cached.
        /// </summary>
        bool IsCacheable { get; }
    }


    public interface ISpecification<T> : ISpecificationGeneric<T> where T : EntityBase
    {
    }
}