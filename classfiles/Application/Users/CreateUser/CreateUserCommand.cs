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

            var activationUrl =
                $"https://your-frontend-domain.com/activate?userId={newuserCreated.Id}&token={Uri.EscapeDataString(rawToken)}";

            var outbox = new Domain.System_Related.EmailOutbox.EmailOutbox(
                //id: 1, // IMPORTANT: try to remove
                to: newUser.Email,
                subject: "Activate your account",
                bodyHtml: $@"<p>Activate: <a href=""{activationUrl}"">link</a></p>",
                type: "activation",
                userId: newUser.Id
            );

            _unitOfWork.EmailOutbox.Add(outbox);
            await _unitOfWork.SaveChanges();

            return newuserCreated.Id;
        }
    }
}
