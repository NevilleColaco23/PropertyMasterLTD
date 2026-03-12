using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Users.DTO;

namespace MyWarehouse.Application.Common.Users
{
    public class GetUsersQuery : ListQueryModel<GetUserDTO>
    {
        // No additional filters needed for now
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IListResponseModel<GetUserDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IListResponseModel<GetUserDTO>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            // Get all users from Users collection
            var users = _unitOfWork.Users?.GetListBy<GetUserDTO>(MongoCollections.UsersCollection) ?? new List<GetUserDTO>();

            // Filter to only active, confirmed users
            var activeUsers = users.Where(u => u.EmailConfirmed && !u.LockoutEnabled).ToList();

            var response = new ListResponseModel<GetUserDTO>
            {
                PageIndex = 1,
                PageSize = activeUsers.Count,
                PageCount = 1,
                RowCount = activeUsers.Count,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = activeUsers.Count,
                Results = activeUsers
            };

            return await Task.FromResult(response);
        }
    }
}
