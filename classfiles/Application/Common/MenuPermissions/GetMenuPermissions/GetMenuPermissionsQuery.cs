using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;

namespace MyWarehouse.Application.Common.MenuPermissions
{
    public class GetMenuPermissionsQuery : ListQueryModel<GetMenuPermissionDTO>
    {
        public int? UserId { get; init; }
        public int? MenuId { get; init; }
    }

    public class GetMenuPermissionsQueryHandler : IRequestHandler<GetMenuPermissionsQuery, IListResponseModel<GetMenuPermissionDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMenuPermissionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IListResponseModel<GetMenuPermissionDTO>> Handle(GetMenuPermissionsQuery request, CancellationToken cancellationToken)
        {
            // Get all permissions from MenuPermissions collection
            var permissions = _unitOfWork.MenuPermissions?.GetListBy<GetMenuPermissionDTO>(MongoCollections.MenuPermissionsCollection) ?? new List<GetMenuPermissionDTO>();

            // Apply filters if provided
            if (request.UserId.HasValue)
            {
                permissions = permissions.Where(p => p.UserId == request.UserId.Value).ToList();
            }

            if (request.MenuId.HasValue)
            {
                permissions = permissions.Where(p => p.MenuId == request.MenuId.Value).ToList();
            }

            var response = new ListResponseModel<GetMenuPermissionDTO>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = permissions.Count,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = permissions.Count,
                Results = permissions
            };

            return await Task.FromResult(response);
        }
    }

    public class GetMenuPermissionByIdQuery : IRequest<GetMenuPermissionDTO?>
    {
        public int Id { get; init; }
    }

    public class GetMenuPermissionByIdQueryHandler : IRequestHandler<GetMenuPermissionByIdQuery, GetMenuPermissionDTO?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMenuPermissionByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetMenuPermissionDTO?> Handle(GetMenuPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            var permission = await _unitOfWork.MenuPermissions.GetByIdAsync(request.Id);

            if (permission == null)
                return null;

            // Map to DTO
            return new GetMenuPermissionDTO
            {
                Id = permission.Id,
                UserId = permission.UserId,
                MenuId = permission.MenuID,
                AccessLevel = permission.AccessLevel,
                From = permission.From,
                To = permission.To
            };
        }
    }
}
