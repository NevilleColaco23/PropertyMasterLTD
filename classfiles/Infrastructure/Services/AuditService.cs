using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Audit;
using MyWarehouse.Domain.Common.Audit;
using System.Text.Json;

namespace MyWarehouse.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogCreate<T>(string collectionName, int documentId, T newValue, int userId, string username = null)
        {
            var auditLog = new AuditLog
            {
                CollectionName = collectionName,
                DocumentId = documentId,
                Operation = "Create",
                UserId = userId,
                Username = username ?? userId.ToString(),
                Timestamp = DateTime.UtcNow,
                OldValue = null,
                NewValue = JsonSerializer.Serialize(newValue, new JsonSerializerOptions { WriteIndented = true })
            };

            await _unitOfWork.AuditLogs.Add(auditLog);
            await _unitOfWork.SaveChanges();
        }

        public async Task LogUpdate<T>(string collectionName, int documentId, T oldValue, T newValue, int userId, string username = null)
        {
            var auditLog = new AuditLog
            {
                CollectionName = collectionName,
                DocumentId = documentId,
                Operation = "Update",
                UserId = userId,
                Username = username ?? userId.ToString(),
                Timestamp = DateTime.UtcNow,
                OldValue = JsonSerializer.Serialize(oldValue, new JsonSerializerOptions { WriteIndented = true }),
                NewValue = JsonSerializer.Serialize(newValue, new JsonSerializerOptions { WriteIndented = true })
            };

            await _unitOfWork.AuditLogs.Add(auditLog);
            await _unitOfWork.SaveChanges();
        }

        public async Task LogDelete<T>(string collectionName, int documentId, T oldValue, int userId, string username = null)
        {
            var auditLog = new AuditLog
            {
                CollectionName = collectionName,
                DocumentId = documentId,
                Operation = "Delete",
                UserId = userId,
                Username = username ?? userId.ToString(),
                Timestamp = DateTime.UtcNow,
                OldValue = JsonSerializer.Serialize(oldValue, new JsonSerializerOptions { WriteIndented = true }),
                NewValue = null
            };

            await _unitOfWork.AuditLogs.Add(auditLog);
            await _unitOfWork.SaveChanges();
        }
    }
}
