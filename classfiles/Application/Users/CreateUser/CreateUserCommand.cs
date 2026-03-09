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
            #region Save user
            var newUser = new Domain.Users.Users(
             username: request.UserName.Trim(),
             email: request.Email.Trim(),
             password: request.Password,
             phoneNumber: request.PhoneNumber
             );

            var newuserCreated = await _unitOfWork.Users.Add(newUser);
            await _unitOfWork.SaveChanges();
            #endregion

            // NOTE: Token generation and email sending are now BOTH handled by UserService.SignUp
            // This ensures the token in the email matches the hash stored in the database

            return newuserCreated.Id;
        }
    }
}
