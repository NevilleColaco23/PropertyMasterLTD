using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.Posts;

namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories
{
    public interface IPostsRepository : IRepository<Post, int>
    {
        Task<List<Post>> GetRecentAsync(int limit);
    }
}
