using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Users.CreateUser
{
    public class CreateUserCommand : IRequest<int>
    {
        public int Id { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
        public string PhoneNumber { get; init; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var newUser = new Domain.Users.Users(
             username: request.UserName.Trim(),
             email: request.Email.Trim(),
             password: request.Password,
             phoneNumber: request.PhoneNumber
             );

            var newuserCreated = _unitOfWork.Users.Add(newUser);
            await _unitOfWork.SaveChanges();

            var test = newUser.Id;
            return newuserCreated.Id;
        }
    }
}
