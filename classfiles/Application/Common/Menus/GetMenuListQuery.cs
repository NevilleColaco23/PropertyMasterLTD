using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Domain;
using System.Data;
using MyWarehouse.Application.Common.Menus.MenuQueries;
using MyWarehouse.Application.Dependencies.Services;

namespace MyWarehouse.Application.Common.Menus
{
    public class GetMenuListQuery : ListQueryModel<GetMenuPermissionMappingListDTO>
    {
        public int userId { get; init; }
    }

    public class GetMenuListQueryHandler : IRequestHandler<GetMenuListQuery, IListResponseModel<GetMenuPermissionMappingListDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMenuListQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IListResponseModel<GetMenuPermissionMappingListDTO>> Handle(GetMenuListQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            var menuList = _unitOfWork.MenuPermissions?.GetListBy<GetMenuPermissionMappingListDTO>(MongoCollections.MenuPermissionsCollection
            ,new GetMenuListQueryByUserId(1)); //currentUserService == null ? 1 : Convert.ToInt32(currentUserService.UserId

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
