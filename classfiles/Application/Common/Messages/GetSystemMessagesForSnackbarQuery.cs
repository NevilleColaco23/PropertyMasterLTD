using MyWarehouse.Application.Common.Bookings.BookingsQuery;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Common.Messages
{
    public class GetSystemMessagesForSnackbarQuery : ListQueryModel<GetSystemMessagesDTO>
    {
        public string BookingId { get; init; }
    }

    public class GetSystemMessagesForSnackbarQueryHandler : IRequestHandler<GetSystemMessagesForSnackbarQuery, IListResponseModel<GetSystemMessagesDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSystemMessagesForSnackbarQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IListResponseModel<GetSystemMessagesDTO>> Handle(GetSystemMessagesForSnackbarQuery request,
            CancellationToken cancellationToken)
        {
            DataTable templateTable = new();

            var menuList = _unitOfWork.SystemMessages?.GetListBy<GetSystemMessagesDTO>(MongoCollections.SystemMessagesCollection
                      , new GetBookingsMongoQuery(request.BookingId,request.SearchItem,request.PageIndex,request.PageSize,request.OrderBy,request.ActiveSortDirection));

            var response = new ListResponseModel<GetSystemMessagesDTO>
            {
                PageIndex = 1,
                PageSize = request.PageSize,
                PageCount = 1,
                RowCount = 1,
                ActiveFilter = request.Filter,
                ActiveOrderBy = request.OrderBy,
                FirstRowOnPage = 1,
                LastRowOnPage = 1,
                Results = menuList ?? new List<GetSystemMessagesDTO>()
            };

            return await Task.FromResult(response);
        }
    }

}
