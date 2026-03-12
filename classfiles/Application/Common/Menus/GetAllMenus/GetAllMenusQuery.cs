using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Application.Common.Menus.DTO;

namespace MyWarehouse.Application.Common.Menus
{
    public class GetAllMenusQuery : ListQueryModel<GetAllMenusDTO>
    {
    }

    public class GetAllMenusQueryHandler : IRequestHandler<GetAllMenusQuery, IListResponseModel<GetAllMenusDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllMenusQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IListResponseModel<GetAllMenusDTO>> Handle(GetAllMenusQuery request, CancellationToken cancellationToken)
        {
            // Get all menus from the Menus collection
            var menus = _unitOfWork.Menus?.GetListBy<GetAllMenusDTO>(MongoCollections.MenuCollection) ?? new List<GetAllMenusDTO>();

            var response = new ListResponseModel<GetAllMenusDTO>
            {
                PageIndex = 1,
                PageSize = menus.Count,
                PageCount = 1,
                RowCount = menus.Count,
                FirstRowOnPage = 1,
                LastRowOnPage = menus.Count,
                Results = menus
            };

            return await Task.FromResult(response);
        }
    }
}
