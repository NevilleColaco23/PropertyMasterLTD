using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Menus.MenuQueries;
using System.Data;
using MyWarehouse.Application.Common.Menus.DTO;

namespace MyWarehouse.Application.Common.Menus
{
    public class GetInitialDataForLandingPageQuery : ListQueryModel<GetMenuPermissionMappingListDTO>
    {
        public int userId { get; init; }
    }

    public class GetInitialDataForLandingPageQueryHandler : IRequestHandler<GetInitialDataForLandingPageQuery, IListResponseModel<GetMenuPermissionMappingListDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetInitialDataForLandingPageQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IListResponseModel<GetMenuPermissionMappingListDTO>> Handle(GetInitialDataForLandingPageQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            var menuList = _unitOfWork.MenuPermissions?.GetListBy<GetMenuPermissionMappingListDTO>(MongoCollections.MenuPermissionsCollection
            , new GetMenuListQueryByUserId(1));

            var response = new ListResponseModel<GetMenuPermissionMappingListDTO>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = menuList ?? new List<GetMenuPermissionMappingListDTO>()
            };

            return await Task.FromResult(response);
        }
    }

}
