using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Domain;
using System.Data;
using MyWarehouse.Application.Common.Menus.MenuQueries;

namespace MyWarehouse.Application.Common.Menus
{
    public class GetMenuListQuery : ListQueryModel<GetMenuListDTO>
    {
        public TransactionType? Type { get; init; }
    }

    public class GetMenuListQueryHandler : IRequestHandler<GetMenuListQuery, IListResponseModel<GetMenuListDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMenuListQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IListResponseModel<GetMenuListDTO>> Handle(GetMenuListQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            var menuList = _unitOfWork.Menus?.GetListBy<GetMenuListDTO>(MongoCollections.MenuCollection
                      , new GetMenuListQueryByUserId(1)); //TODO : hardcoded user id

            var response = new ListResponseModel<GetMenuListDTO>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = menuList ?? new List<GetMenuListDTO>()
            };

            return await Task.FromResult(response);
        }
    }

}
