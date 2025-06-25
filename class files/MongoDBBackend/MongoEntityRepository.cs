using System.Linq.Expressions;

namespace MongoDBBackend
{
    // This class is just a T specific wrapper over MongoRepository
    internal class MongoEntityRepository<T> : IRepositoryMongo<T> where T : EntityBase
    {
        private readonly MongoRepository _repository;

        internal MongoEntityRepository(IPersistenceContext context, string tableName = null)
        {
            _repository = new MongoRepository(context, tableName);
        }

        public void Delete(T item)
        {
            _repository.Delete(item);
        }

        public int DeleteBy(ISpecification<T> specification)
        {
            return _repository.DeleteBy(specification.IsSatisfiedBy());
        }

        public void DeleteById<TId>(TId id)
        {
            _repository.DeleteById<T, TId>(id);
        }

        public int GetCount(bool includeDeleted = false)
        {
            return _repository.GetCount<T>(includeDeleted);
        }

        public int GetCountBy(ISpecification<T> specification)
        {
            return _repository.GetCountBy(specification.IsSatisfiedBy());
        }

        public IEnumerable<T> GetFutureListBy(ISpecification<T> specification)
        {
            return _repository.GetListBy(specification.IsSatisfiedBy());    // No Future Queries in Mongo, execute normal query
        }

        public T GetItemBy(ISpecification<T> specification)
        {
            return _repository.GetItemBy(specification.IsSatisfiedBy());
        }

        public T GetItemBy(ISpecification<T> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            return _repository.GetItemBy(specification.IsSatisfiedBy(), sortSelector, bAscending, sortSelector2, bAscending2);
        }

        public T GetItemById<TId>(TId id)
        {
            return _repository.GetItemById<T, TId>(id);
        }

        public IList<T> GetItemsByIds<TId>(IEnumerable<TId> ids)
        {
            return _repository.GetItemsByIds<T, TId>(ids);
        }

        public IList<T> GetListAll(bool includeDeleted = false)
        {
            return _repository.GetListAll<T>(includeDeleted);
        }

        public IList<T> GetListBy(ISpecification<T> specification)
        {
            return _repository.GetListBy(specification.IsSatisfiedBy());
        }

        public IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords)
        {
            return _repository.GetListPagedBy(specification.IsSatisfiedBy(), start, numRecords);
        }

        public IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords, out int rowCount)
        {
            return _repository.GetListPagedBy(specification.IsSatisfiedBy(), start, numRecords, out rowCount);
        }

        public IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords, out int rowCount, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            return _repository.GetListPagedBy(specification.IsSatisfiedBy(), start, numRecords, out rowCount, sortSelector, bAscending, sortSelector2, bAscending2);
        }

        public IList<T> GetListPagedBy(ISpecification<T> specification, int start, int numRecords, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            return _repository.GetListPagedBy(specification.IsSatisfiedBy(), start, numRecords, sortSelector, bAscending, sortSelector2, bAscending2);
        }

        public IQueryable<T> GetQueryable()
        {
            return _repository.GetQueryable<T>();
        }

        public IQueryable<T> GetQueryable(ISpecification<T> specification)
        {
            return _repository.GetQueryable(specification.IsSatisfiedBy());
        }

        public IList<T> GetSortedListBy(ISpecification<T> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true, Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true)
        {
            return _repository.GetSortedListBy(specification.IsSatisfiedBy(), sortSelector, bAscending, sortSelector2, bAscending2);
        }

        public void Save(T item)
        {
            _repository.Save(item);
        }
    }
}
