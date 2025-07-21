using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;
using MyWarehouse.Application.Common.Searchbox.GetSearchResultsMongoQuery;

namespace MyWarehouse.Application.Common.Searchbox
{
    public class GetSearchResultsQuery : ListQueryModel<GetSearchResultsDTO>, IRequest<IListResponseModel<GetSearchResultsDTO>>
    {
        public string SearchText { get; init; }
    }

    public class GetSearchResultsQueryHandler : IRequestHandler<GetSearchResultsQuery, IListResponseModel<GetSearchResultsDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSearchResultsQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IListResponseModel<GetSearchResultsDTO>> Handle(GetSearchResultsQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            var menuList = _unitOfWork.Menus?.GetListBy<GetSearchResultsDTO>(MongoCollections.MenuCollection
            , new MyWarehouse.Application.Common.Searchbox.GetSearchResultsMongoQuery.GetSearchResultsMongoQuery(request.SearchText, string.Empty));//request.userId

            var response = new ListResponseModel<GetSearchResultsDTO>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = menuList ?? new List<GetSearchResultsDTO>()
            };

            return await Task.FromResult(response);
        }
    }

}
