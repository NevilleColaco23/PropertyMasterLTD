namespace MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

public interface IListResponseModel<T>
{
    int PageIndex { get; }
    int PageSize { get; }

    int PageCount { get; }
    int RowCount { get; }

    string? ActiveFilter { get; }
    string? ActiveOrderBy { get; }
    int ActiveSortDirection { get; }

    int FirstRowOnPage { get; }
    int LastRowOnPage { get; }
    string SearchItem { get; }

    IEnumerable<T> Results { get; set; }
}

public class ListResponseModel<T> : IListResponseModel<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public int RowCount { get; set; }
    public int TotalRowCount { get; set; }
    public string? ActiveFilter { get; set; }
    public string? ActiveOrderBy { get; set; }
    public int ActiveSortDirection { get; set; }
    public int FirstRowOnPage { get; set; }
    public int LastRowOnPage { get; set; }
    public string SearchItem { get; set; }
    public IEnumerable<T> Results { get; set; } = new List<T>();
}

