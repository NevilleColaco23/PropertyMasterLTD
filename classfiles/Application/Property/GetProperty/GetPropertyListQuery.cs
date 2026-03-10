using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Property.PropertyQueries;
using System.Data;
using MyWarehouse.Application.Dependencies.Services;

namespace MyWarehouse.Application.Property.GetProperty
{
    public class GetPropertyListQuery : ListQueryModel<GetPropertyDto>, IRequest<IListResponseModel<GetPropertyDto>>
    {
    }

    public class GetPropertyListQueryHandler : IRequestHandler<GetPropertyListQuery, IListResponseModel<GetPropertyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetPropertyListQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            this._currentUserService = currentUserService;
        }

        public async Task<IListResponseModel<GetPropertyDto>> Handle(GetPropertyListQuery request,
            CancellationToken cancellationToken)
        {
            string userIdString = _currentUserService.UserId ?? "0";

            // Query Property collection (correct!) and pass user ID to filter
            var propertyListTest = _unitOfWork.Properties?.GetListBy<GetPropertyDto>(MongoCollections.PropertyCollection
                      , new GetPropertyQueryByUserIdUsingMongoQueryString(
                          int.TryParse(userIdString, out int parsedUserId) ? parsedUserId : 0, string.Empty));

            var response = new ListResponseModel<GetPropertyDto>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = propertyListTest ?? new List<GetPropertyDto>()
            };

            return await Task.FromResult(response);
        }
    }
}
