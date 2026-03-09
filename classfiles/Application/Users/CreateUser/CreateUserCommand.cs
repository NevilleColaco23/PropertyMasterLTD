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


            var test = newuserCreated.Id; //TODO: 

            // Generate token and store HASH on user
            var rawToken = EmailConfirmationToken.GenerateRawToken();
            var tokenHash = EmailConfirmationToken.HashToken(rawToken);

            newuserCreated.EmailConfirmationTokenHash = tokenHash;
            newuserCreated.EmailConfirmationTokenExpiresAtUtc = DateTime.UtcNow.AddHours(24);


            await _unitOfWork.Users.Update(newuserCreated); 
            await _unitOfWork.SaveChanges();

            // NOTE: Email sending is now handled by UserService.SignUp
            // No need to create EmailOutbox here - the UserService creates it with proper HTML template

            return newuserCreated.Id;
        }
    }
}
