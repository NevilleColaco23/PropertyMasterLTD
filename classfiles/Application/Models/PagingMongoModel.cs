
namespace MyWarehouse.Application.Models
{
    public class PagingMongoModel
    {
        public class PagedResult<T>
        {
            public List<T> results { get; set; }
            public List<CountResult> totalCount { get; set; }
        }

        public class CountResult
        {
            public int count { get; set; }
        }
    }
}
