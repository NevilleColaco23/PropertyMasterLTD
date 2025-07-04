using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Property.PropertyQueries;
using System.Data;
using MyWarehouse.Application.Users.GetUsersByUserid;

namespace MyWarehouse.Application.Property.GetProperty
{
    public class GetPropertyListQuery : ListQueryModel<GetPropertyDto>
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
            DataTable templateTable = new();

            //var testDatabase = _unitOfWork.Properties? //This is working
            //    .GetDataTablePaged(MongoCollections.PropertyCollection, new GetPropertyQuery(100,string.Empty), null,
            //        0, request.PageSize, out var totalCount, "_id", false);

            var propertyListTest = _unitOfWork.Properties?.GetListBy<GetPropertyDto>(MongoCollections.PropertyCollection
                      , new GetPropertyQueryByUserId(1, string.Empty)); //TODO : hardcoded user id

            //return await _unitOfWork.Properties?.GetProjectedListAsync(request, readOnly: true)!; to get all properties

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
