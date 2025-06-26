using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Partners.GetPartnersList;
using System;

namespace MyWarehouse.Application.Bookings.GetBookings
{
    public class GetBookingsListQuery : IRequestHandler<ListQueryModel<PartnerDto>, IListResponseModel<PartnerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBookingsListQuery(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public Task<IListResponseModel<PartnerDto>> Handle(ListQueryModel<PartnerDto> request, CancellationToken cancellationToken)
            => _unitOfWork.Partners.GetProjectedListAsync(request, readOnly: true);
    }
}
