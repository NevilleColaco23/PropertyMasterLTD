using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.Posts;

namespace MyWarehouse.Application.Posts.AcknowledgeComment;

public class AcknowledgeCommentCommand : IRequest<Unit>
{
    public int PostId { get; init; }
    public int CommentId { get; init; }
    public string UserName { get; init; } = null!;
}

public class AcknowledgeCommentCommandHandler : IRequestHandler<AcknowledgeCommentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AcknowledgeCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(AcknowledgeCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Posts!.GetByIdAsync(request.PostId);
        if (post == null)
        {
            throw new EntityNotFoundException(nameof(Post), request.PostId);
        }

        var comment = post.Comments.FirstOrDefault(c => c.Id == request.CommentId);
        if (comment == null)
        {
            throw new EntityNotFoundException(nameof(PostComment), request.CommentId);
        }

        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        if (!comment.Acknowledgements.Any(a => a.UserId == currentUserId))
        {
            comment.Acknowledgements.Add(new Acknowledgement
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
