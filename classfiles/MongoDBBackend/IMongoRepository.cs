using System.Data;
using System.Linq.Expressions;

namespace MongoDBBackend
{
    public interface IMongoRepository
    {
        T GetItemBy<T>(Expression<Func<T, bool>> specification);

        T GetItemBy<T>(Expression<Func<T, bool>> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        T GetItemById<T, TId>(TId id);

        IList<T> GetListAll<T>(bool includeDeleted = false);

        IList<T> GetListBy<T>(Expression<Func<T, bool>> specification);

        IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords);

        IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords, out int rowCount);

        IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords, out int rowCount, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        IList<T> GetListPagedBy<T>(Expression<Func<T, bool>> specification, int start, int numRecords, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        IList<T> GetListBy<T>(string tableName, INamedQueryMongo filterQuery = null);

        IList<T> GetListByFacet<T>(string tableName, INamedQueryMongo filterQuery, INamedQueryMongo summaryQuery, int start, int numRecords, out Dictionary<string, object> summary,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true);

        IQueryable<T> GetQueryable<T>();

        IQueryable<T> GetQueryable<T>(Expression<Func<T, bool>> specification);

        IList<T> GetSortedListBy<T>(Expression<Func<T, bool>> specification, Expression<Func<T, object>> sortSelector, bool bAscending = true,
            Expression<Func<T, object>> sortSelector2 = null, bool bAscending2 = true);

        // Will return results of multiple queries joined together with "Union"
        DataTable GetDataTableUnion(string tableName, List<INamedQueryMongo> queries);

        DataSet GetDataSetByFacet(string tableName, List<INamedQueryMongo> queries);

        // Caution: Passing null filter query will load the whole table
        DataTable GetDataTable(string tableName, INamedQueryMongo filterQuery, DataTable template = null,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true);  // Sorting is optional

        // Caution: Passing null filter query will load the whole table
        DataSet GetDataSet(string tableName, INamedQueryMongo filterQuery, DataTable template = null);

        // Caution: Passing null filter query will load the whole table, if not limited by numRecords
        DataTable GetDataTablePaged(string tableName, INamedQueryMongo filterQuery, DataTable template, int start, int numRecords, out int totalCount,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true);

        // Caution: Passing null filter query will load the whole table
        List<Dictionary<string, object>> GetDictionary(string tableName, INamedQueryMongo filterQuery = null,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true);    // Sorting is optional

        // Caution: Passing null filter query will load the whole table, if not limited by numRecords
        List<Dictionary<string, object>> GetDictionaryPaged(string tableName, INamedQueryMongo filterQuery, int start, int numRecords, out int totalRecords,
            string sortField1 = null, bool bAscending = true, string sortField2 = null, bool bAscending2 = true);    // Sorting is optional

        List<Dictionary<string, object>> GetDictionaryByAggregate(string tableName, INamedQueryMongo filterQuery = null);

        // Caution: Do not use this method to save data table.
        string Save<T>(T item);

        void Save<T>(string tableName, IEnumerable<T> data, bool merge);

        void Save(DataTable dataTable, bool merge, bool insertNullFields = false);

        // Saves multiple documents, inserts if not found, will merge or replace as per the merge flag
        void Save(string tableName, List<Dictionary<string, object>> data, bool merge);

        // Saves one document specified by recordId, inserts if not found, will merge or replace as per the merge flag
        void Save(string tableName, string recordId, Dictionary<string, object> data, bool merge);

        // Bulk Inserts new records
        void BulkInsert(DataTable dt, bool insertNullFields = false);

        void Update(string tableName, INamedQueryMongo filterQuery);

        void Delete<T>(T item);

        int DeleteBy<T>(Expression<Func<T, bool>> specification);

        void DeleteById<T, TId>(TId id);

        long DeleteMany(string tableName, INamedQueryMongo namedQuery);

        int GetCount<T>(bool includeDeleted = false);

        int GetCountBy<T>(Expression<Func<T, bool>> specification);

        List<TF> GetDistinct<TF>(string tableName, INamedQueryMongo filterQuery, string fieldName);

        void ExecuteQuery(string tableName, INamedQueryMongo filterQuery);

        void DropCollection(string tableName);

        //Task<int> GetNextIncrementedValue<T>(string collectionName);
    }
}
