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

            // Queue activation email (outbox)
            // NOTE: you'll later replace token=TODO with real token/link generation
            var activationUrl =
                $"https://your-frontend-domain.com/activate?email={Uri.EscapeDataString(newUser.Email)}&token=TODO";

            var outbox = new Domain.System_Related.EmailOutbox.EmailOutbox(
                id: 1, // IMPORTANT: only if your EmailOutbox uses int _id; otherwise remove this for ObjectId
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
