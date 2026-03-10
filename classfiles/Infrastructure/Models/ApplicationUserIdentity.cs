using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MyWarehouse.Infrastructure.Models
{
    [CollectionName("Users")]
    public class ApplicationUserIdentity : MongoIdentityUser<int>
    {
        public List<PropertyAccess> PropertyAccessList { get; set; } = new List<PropertyAccess>();
    }
    public class PropertyAccess
    {
        public int Id { get; set; }  // Changed from PropertyID to Id to match MongoDB schema
        public bool IsActive { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
