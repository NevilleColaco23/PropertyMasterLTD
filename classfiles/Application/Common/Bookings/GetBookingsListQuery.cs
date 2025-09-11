using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;
using MyWarehouse.Application.Common.Bookings.BookingsQuery;

namespace MyWarehouse.Application.Common.Bookings
{
    public class GetBookingsListQuery : ListQueryModel<GetBookingsListDTO>
    {
        public string BookingId { get; init; }
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

            var menuList = _unitOfWork.Bookings?.GetListBy<GetBookingsListDTO>(MongoCollections.BookingsCollection
                      , new GetBookingsMongoQuery(request.BookingId));

            var response = new ListResponseModel<GetBookingsListDTO>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = menuList ?? new List<GetBookingsListDTO>()
            };

            return await Task.FromResult(response);
        }
    }

}
