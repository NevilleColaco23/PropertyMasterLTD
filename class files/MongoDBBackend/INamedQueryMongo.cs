using System.Data;

namespace MongoDBBackend
{
    /// <summary>
    /// A SQl query, having a name by which we identify it.
    /// The repository will take the Sql Query string, and replace the parameters while executing it.
    /// </summary>
    public interface INamedQueryMongo
    {
        /// <summary>
        /// Returns the SQL query string
        /// </summary>
        string QueryStr { get; }

        /// <summary>
        /// Returns the CommandType -> Text / StoredProcedure / TableDirect
        /// </summary>
        CommandType CommandType { get; }

        /// <summary>
        /// The Sql query's parameter Name & value list
        /// </summary>
        IReadOnlyList<NamedQueryParameter> Parameters { get; }
    }

    public class NamedQueryParameter
    {
        /// <summary>
        /// Creates an Input parameter with the specified value
        /// </summary>
        public NamedQueryParameter(string name, object value)
        {
            Name = name;
            Value = value ?? DBNull.Value;
            Direction = ParameterDirection.Input;
        }

        /// <summary>
        /// Create an Output parameter of the specified type
        /// </summary>
        public NamedQueryParameter(string name, SqlDbType dataType, int size = 0)
        {
            Name = name;
            DataType = dataType;
            Size = size;
            Direction = ParameterDirection.Output;
        }

        /// <summary>
        /// Creates an Input - Output parameter with the specified value and type
        /// </summary>
        public NamedQueryParameter(string name, object value, SqlDbType dataType, int size = 0)
        {
            Name = name;
            Size = size;
            DataType = dataType;
            Value = value ?? DBNull.Value;
            Direction = ParameterDirection.InputOutput;
        }

        public string Name { get; }

        public object Value { get; set; }

        public int Size { get; }

        public ParameterDirection Direction { get; }

        public SqlDbType DataType { get; }
    }

    public static class NamedQueryExtensions
    {
        public static NamedQueryParameter ParameterByName(this INamedQueryMongo query, string name)
        {
            return query.Parameters.First(x => x.Name == name);
        }
    }
}
