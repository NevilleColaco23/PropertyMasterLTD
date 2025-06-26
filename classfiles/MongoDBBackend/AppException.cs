namespace MongoDBBackend
{
    /// <summary>
    /// An Exception class defined by us.
    /// Exception messages in this exception are user friendly and can be shown in UI.
    /// </summary>
    public class AppException : Exception
    {
        public AppException()
        {
        }

        public AppException(string message)
            : base(message)
        {
        }

        public AppException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
