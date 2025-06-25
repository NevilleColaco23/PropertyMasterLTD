using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MyWarehouse.Infrastructure.Models
{
    [CollectionName("Roles")]

    public class ApplicationRoleIdentity : MongoIdentityRole<int>
    {
    }
}
