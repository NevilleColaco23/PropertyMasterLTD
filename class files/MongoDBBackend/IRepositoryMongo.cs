using System.Linq.Expressions;
using MyWarehouse.Domain.Common;

namespace MongoDBBackend
{
    /// <summary>
    /// Repository interface, providing various CRUD functionality
    /// </summary>
    /// <typeparam name="T">The entity on which the functions act</typeparam>
    public interface IRepositoryMongo<T> where T : EntityBase
    {
        /// <summary>
        /// Returns IQueryable, upon which further Filter/Sort/Select operations can be performed. 
        /// Use this method only if the other IRepository functions do not serve the purpose
        /// </summary>
        /// <returns>IQueryable</returns>
        IQueryable<T> GetQueryable();

        /// <summary>
        /// Returns IQueryable, upon which further Filter/Sort/Select operations can be performed. 
        /// </summary>
        /// <param name="specification">The specification by which to filter records</param>
        /// <returns>IQueryable</returns>
        IQueryable<T> GetQueryable(ISpecification<T> specification);

        /// <summary>
        /// Returns an entity by its Id
        /// </summary>
        /// <typeparam name="TId">Id type</typeparam>
        /// <param name="id">Id</param>
        /// <returns>Entity having the specified Id, null if not found</returns>
        T GetItemById<TId>(TId id);

        /// <summary>
        /// Returns list of items by their Ids
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        IList<T> GetItemsByIds<TId>(IEnumerable<TId> ids);

        /// <summary>
        /// Returns first entity matching specification.
        /// </summary>
        /// <param name="specification">The specification by which to filter records</param>
        /// <returns>First entity matching the specification, null if no items found.</returns>
        T GetItemBy(ISpecification<T> specification);

        /// <summary>
        /// Returns Top 1 item matching specification after sorting.
        /// </summary>
        /// <param name="specification">The specfication on which to filter records</param>
        /// <param name="sortSelector">Lambda expression, specifying the first property to sort upon</param>
        /// <param name="bAscending">True if the first sort is ascending, false if descending</param>
        /// <param name="sortSelector2">Lambda expression, specifying the second property to sort upon</param>
        /// <param name="bAscending2">True if the second sort is ascending, false if descending</param>
        /// <returns></returns>
        T GetItemBy(ISpecification<T> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        /// <summary>
        /// Returns a list of ALL items.
        /// </summary>
        /// <returns>A list of ALL items.</returns>
        IList<T> GetListAll(bool includeDeleted = false);

        /// <summary>
        /// Returns a list of items matching given specification.
        /// </summary>
        /// <param name="specification">The specfication on which to filter records</param>
        /// <returns>A list of items matching given specification</returns>
        IList<T> GetListBy(ISpecification<T> specification);

        /// <summary>
        /// Returns a part list of items matching given specification.
        /// </summary>
        /// <param name="specification">The specification on which to filter records</param>
        /// <param name="start">The record number to start from</param>
        /// <param name="numRecords">The number of records to return</param>
        /// <returns>A part list of items matching given specification.</returns>
        IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords);

        /// <summary>
        /// Returns a part list of items matching sent expression, along with the row count.
        /// </summary>
        /// <param name="specification">The specification on which to filter records</param>
        /// <param name="start">The record number to start from</param>
        /// <param name="numRecords">The number of records to return</param>
        /// <param name="rowCount"> Out parameter, the total number of records that matched the specification</param>
        /// <returns>A part list of items matching given specification.</returns>
        IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords, out int rowCount);

        /// <summary>
        /// Returns a sorted part list of items matching the specification, along with the row count.
        /// </summary>
        /// <param name="specification">The specification on which to filter records</param>
        /// <param name="start">The record number to start from</param>
        /// <param name="numRecords">The number of records to return</param>
        /// <param name="rowCount"> Out parameter, the total number of records that matched the specification</param>
        /// <param name="sortSelector">Lambda expression, specifying the first property to sort upon</param>
        /// <param name="bAscending">True if the first sort is ascending, false if descending</param>
        /// <param name="sortSelector2">Lambda expression, specifying the second property to sort upon</param>
        /// <param name="bAscending2">True if the second sort is ascending, false if descending</param>
        /// <returns>A sorted part list of items matching the specification</returns>
        IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords, out int rowCount,
            Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        /// <summary>
        /// Returns a sorted part list of items matching the specification, along with the row count.
        /// </summary>
        /// <param name="specification">The specification on which to filter records</param>
        /// <param name="start">The record number to start from</param>
        /// <param name="numRecords">The number of records to return</param>
        /// <param name="sortSelector">Lambda expression, specifying the first property to sort upon</param>
        /// <param name="bAscending">True if the first sort is ascending, false if descending</param>
        /// <param name="sortSelector2">Lambda expression, specifying the second property to sort upon</param>
        /// <param name="bAscending2">True if the second sort is ascending, false if descending</param>
        /// <returns>A sorted part list of items matching the specification</returns>
        IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords,
            Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        IList<T> GetSortedListBy(ISpecification<T> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        /// <summary>
        /// Gets the count of items in the table.
        /// </summary>
        /// <returns>The count of items in the table.</returns>
        int GetCount(bool includeDeleted = false);

        /// <summary>
        /// Gets the count of items that matches the specification.
        /// </summary>
        /// <returns>The count of items that matches the specification.</returns>
        int GetCountBy(ISpecification<T> specification);

        /// <summary>
        /// Gets a list of items as IEnumerable, with deferred execution
        /// </summary>
        /// <param name="specification">The specification on which to filter the records</param>
        /// <returns>IEnumerable with deferred execution</returns>
        IEnumerable<T> GetFutureListBy(ISpecification<T> specification);

        /// <summary>
        /// Deletes the given item from the database.
        /// </summary>
        /// <param name="item">The item to delete</param>
        void Delete(T item);

        /// <summary>
        /// Deletes the entity having the given Id
        /// </summary>
        /// <param name="id">The Id of the entity to delete</param>
        void DeleteById<TId>(TId id);

        /// <summary>
        /// Delete records matching the given specification.
        /// </summary>
        /// <param name="specification">The specification on which to filter the records</param>
        /// <returns>Number of records deleted</returns>
        int DeleteBy(ISpecification<T> specification);

        /// <summary>
        /// Saves Transient (Unattached) entity or Updates attached entity.
        /// </summary>
        /// <param name="item">The entity to save</param>
        void Save(T item);
    }
}
