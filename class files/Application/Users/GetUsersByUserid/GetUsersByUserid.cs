using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Property.PropertyQueries;
using System.Data;
using MyWarehouse.Application.Users.GetUsersByUserid;

namespace MyWarehouse.Application.Users.GetUsers
{
    public class GetUsersByUseridQuery : ListQueryModel<GetUsersbyUserIdDto>
    {
    }

    public class GetUsersByUseridQueryHandler : IRequestHandler<GetUsersByUseridQuery, IListResponseModel<GetUsersbyUserIdDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUsersByUseridQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IListResponseModel<GetUsersbyUserIdDto>> Handle(GetUsersByUseridQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            var propertyList = _unitOfWork.Users?.GetListBy<GetUsersbyUserIdDto>(MongoCollections.UsersCollection
                      , new GetPropertyQueryByUserId(1, string.Empty));

            var response = new ListResponseModel<GetUsersbyUserIdDto>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = propertyList ?? new List<GetUsersbyUserIdDto>()
            };

            return await Task.FromResult(response);
        }
    }
}
