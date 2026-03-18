using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions;
using System.Data;
using MyWarehouse.Application.Common.Bookings.BookingsQuery;

namespace MyWarehouse.Application.Common.Bookings
{
    public class GetBookingsListQuery : ListQueryModel<GetBookingsListDTO>
    {
        public string BookingId { get; init; }
        public List<int> PropertyIds { get; init; } = new List<int>();
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }

    public class GetBookingsListQueryHandler : IRequestHandler<GetBookingsListQuery, IListResponseModel<GetBookingsListDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBookingsListQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IListResponseModel<GetBookingsListDTO>> Handle(GetBookingsListQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            // Create the query
            var mongoQuery = new GetBookingsMongoQuery(
                request.BookingId, 
                request.SearchItem, 
                request.PageIndex, 
                request.PageSize, 
                request.OrderBy, 
                request.ActiveSortDirection, 
                request.PropertyIds,
                getAllProperties: false,
                startDate: request.StartDate,
                endDate: request.EndDate);

            // 🔍 DEBUG: Store query debug info in local variables (set breakpoint here to inspect)
            var debugInfo = mongoQuery.GetDebugInfo();
            var debugPipeline = mongoQuery.GetPipelineAsJsonString();

            var menuList = _unitOfWork.Bookings?.GetPagedListBy<GetBookingsListDTO>(MongoCollections.BookingsCollection, mongoQuery);

            // 🔍 DEBUG: Store results info (set breakpoint here to inspect)
            var resultCount = menuList.results.Count;

            // Get total count from MongoDB aggregation (from $facet totalCount)
            var totalRowCount = menuList?.totalCount?.FirstOrDefault()?.count ?? 0;

            var response = new ListResponseModel<GetBookingsListDTO>
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                PageCount = menuList.results.Count,
                RowCount = menuList.results.Count,
                TotalRowCount = totalRowCount,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = menuList.results ?? new List<GetBookingsListDTO>()
            };

            return await Task.FromResult(response);
        }
    }

}
