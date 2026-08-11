using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Dependencies.Services;

namespace MyWarehouse.Application.Posts.DeletePost;

public class DeletePostCommand : IRequest<Unit>
{
    public int Id { get; init; }
}

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeletePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Posts!.GetByIdAsync(request.Id);

        if (post == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Posts.Post), request.Id);
        }

        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        if (post.UserId != currentUserId)
        {
            // Treat as not found to avoid leaking existence of other users' posts.
            throw new EntityNotFoundException(nameof(Domain.Posts.Post), request.Id);
        }

        _unitOfWork.Posts!.Remove(post);

        return Unit.Value;
    }
}
