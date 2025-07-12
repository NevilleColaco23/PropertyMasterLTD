using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Property.PropertyQueries;
using System.Data;

namespace MyWarehouse.Application.Property.GetProperty
{
    public class GetPropertyListQuery : ListQueryModel<GetPropertyDto>, IRequest<IListResponseModel<GetPropertyDto>>
    {
    }

    public class GetPropertyListQueryHandler : IRequestHandler<GetPropertyListQuery, IListResponseModel<GetPropertyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPropertyListQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IListResponseModel<GetPropertyDto>> Handle(GetPropertyListQuery request,
            CancellationToken cancellationToken)
        {
            var propertyListTest = _unitOfWork.Properties?.GetListBy<GetPropertyDto>(MongoCollections.PropertyCollection
                      , new GetPropertyQueryByUserIdUsingMongoQueryString(100, string.Empty)); //TODO : hardcoded user id

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
