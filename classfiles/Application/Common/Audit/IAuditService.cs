namespace MyWarehouse.Application.Common.Audit
{
    public interface IAuditService
    {
        Task LogCreate<T>(string collectionName, int documentId, T newValue, int userId, string username = null);
        Task LogUpdate<T>(string collectionName, int documentId, T oldValue, T newValue, int userId, string username = null);
        Task LogDelete<T>(string collectionName, int documentId, T oldValue, int userId, string username = null);
    }
}
