using System.Data;

namespace MongoDBBackend
{
    /// <summary>
    /// Repository for executing SQL Queries.
    /// </summary>
    public interface INamedQueryRepositoryMongo
    {
        /// <summary>
        /// Returns the list of records, that is the output of a SQL string.
        /// </summary>
        /// <typeparam name="T">Type of objects returned by the Query</typeparam>
        /// <param name="namedQuery">The SQl query to execute</param>
        /// <returns>List of records of type T, which are returned by the Sql query</returns>
        IList<T> GetListBy<T>(INamedQueryMongo namedQuery) where T : class;

        /// <summary>
        /// Returns a list of dynamic objects, that is the output of a Sql string.
        /// </summary>
        /// <param name="namedQuery">The SQl query to execute</param>
        /// <returns>List of records of dynamic objects, which are returned by the Sql query</returns>
        IList<dynamic> GetByNamedQuery(INamedQueryMongo namedQuery);

        /// <summary>
        /// Executes a sql statement or stored procedure which returns no result rows.
        /// </summary>
        /// <param name="namedQuery">The SQl statement to execute</param>
        /// <returns>The number of rows affected by the sql statement</returns>
        long ExecuteNamedQuery(INamedQueryMongo namedQuery);

        /// <summary>
        /// Executes the query and returns the first column of the first row in the resultset. All other rows and columns are ignored.
        /// </summary>
        /// <typeparam name="T">The scalar value to return</typeparam>
        /// <param name="namedQuery"></param>
        /// <returns>The first column of the first row in the resultset</returns>
        T ExecuteScalarQuery<T>(INamedQueryMongo namedQuery) where T : class;

        DataSet GetDataSet(INamedQueryMongo namedQuery);

        DataSet GetDataSet(IEnumerable<INamedQueryMongo> queries);

        DataTable GetDataTable(INamedQueryMongo namedQuery);

        /// <summary>
        /// Executes multiple queries by concatenating them together using 'UNION'. The number of columns in each query should be the same.
        /// If you want to execute queries with different columns output, then use GetDataSet()
        /// </summary>
        /// <param name="queries">List of queries with same number of columns output in each</param>
        /// <returns>DataTable</returns>
        DataTable GetDataTable(IEnumerable<INamedQueryMongo> queries);

        DataTable GetDataTable(string tableName, IEnumerable<string> fieldNames = null, string whereClause = null);

        void SaveDataTable(DataTable dataTable);

        void BulkCopy(DataTable dataTable);
    }
}
