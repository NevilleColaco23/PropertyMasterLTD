using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.Posts;

namespace MyWarehouse.Application.Posts.AcknowledgePost;

public class AcknowledgePostCommand : IRequest<Unit>
{
    public int PostId { get; init; }
    public string UserName { get; init; } = null!;
}

public class AcknowledgePostCommandHandler : IRequestHandler<AcknowledgePostCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AcknowledgePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(AcknowledgePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Posts!.GetByIdAsync(request.PostId);
        if (post == null)
        {
            throw new EntityNotFoundException(nameof(Post), request.PostId);
        }

        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        if (!post.Acknowledgements.Any(a => a.UserId == currentUserId))
        {
            post.Acknowledgements.Add(new Acknowledgement
            {
                UserId = currentUserId,
                UserName = request.UserName.Trim(),
                AcknowledgedAt = DateTime.UtcNow
            });

            await _unitOfWork.Posts!.Update(post, cancellationToken);
        }

        return Unit.Value;
    }
}
