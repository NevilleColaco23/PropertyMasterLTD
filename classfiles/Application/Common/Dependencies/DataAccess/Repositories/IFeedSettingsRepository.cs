using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.SystemSettings;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IFeedSettingsRepository : IRepository<FeedSettings, int>
    {
        Task<FeedSettings?> GetByUserIdAsync(int userId);
        Task<FeedSettings> UpsertAsync(int userId, int maxPostsToShow);
    }
}
