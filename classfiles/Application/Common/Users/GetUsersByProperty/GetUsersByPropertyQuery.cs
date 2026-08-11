using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Users.DTO;

namespace MyWarehouse.Application.Common.Users.GetUsersByProperty
{
    /// <summary>
    /// Returns active users who have access to the given property, for use in
    /// @mention autocomplete on the Team Feed composer/comments.
    /// </summary>
    public class GetUsersByPropertyQuery : IRequest<List<GetUserDTO>>
    {
        public int PropertyId { get; init; }
    }

    public class GetUsersByPropertyQueryHandler : IRequestHandler<GetUsersByPropertyQuery, List<GetUserDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUsersByPropertyQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<GetUserDTO>> Handle(GetUsersByPropertyQuery request, CancellationToken cancellationToken)
        {
            var users = _unitOfWork.Users?.GetListBy<GetUserWithPropertyAccessDTO>(MongoCollections.UsersCollection)
                ?? new List<GetUserWithPropertyAccessDTO>();

            var result = users
                .Where(u => u.EmailConfirmed && !u.LockoutEnabled)
                .Where(u => u.PropertyAccessList != null && u.PropertyAccessList.Any(p => p.IsActive && p.Id == request.PropertyId))
                .Select(u => new GetUserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed,
                    LockoutEnabled = u.LockoutEnabled
                })
                .ToList();

            return Task.FromResult(result);
        }
    }
}
