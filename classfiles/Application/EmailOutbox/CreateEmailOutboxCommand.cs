using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.EmailOutbox
{
    // Command payload to create a queued email outbox record
    public class CreateEmailOutboxCommand : IRequest<int>
    {
        // If you are manually controlling _id as int, keep this.
        // If you switch to auto-increment later, you can remove Id from the command.
        public int Id { get; init; }

        public string Type { get; init; } = "activation";
        public string To { get; init; } = default!;
        public string Subject { get; init; } = default!;
        public string BodyHtml { get; init; } = default!;

        // Optional scheduling fields
        public DateTime? NextRunAtUtc { get; init; }

        public int UserId { get; init; }

    }

    public class CreateEmailOutboxCommandHandler : IRequestHandler<CreateEmailOutboxCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateEmailOutboxCommandHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<int> Handle(CreateEmailOutboxCommand request, CancellationToken cancellationToken)
        {
            // Basic input cleanup
            var to = request.To.Trim();
            var subject = request.Subject.Trim();

            var outbox = new Domain.System_Related.EmailOutbox.EmailOutbox(
                id: request.Id,
                to: to,
                subject: subject,
                bodyHtml: request.BodyHtml,
                type: request.Type,
                userId: request.UserId
            );

            if (request.NextRunAtUtc.HasValue)
                outbox.NextRunAtUtc = request.NextRunAtUtc.Value;

            var created = _unitOfWork.EmailOutbox.Add(outbox);
            await _unitOfWork.SaveChanges();

            return created.Id;
        }
    }
}