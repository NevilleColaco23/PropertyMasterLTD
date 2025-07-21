namespace MyWarehouse.Application.Common.Searchbox
{
    public class GetSearchResultsDTO
    {
        public int? Id { get; set; }
        public List<string> searchedItem { get; set; }
    }
}
