using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.Posts;

namespace MyWarehouse.Application.Posts.GetPostsList;

public class GetPostsListQuery : IRequest<List<PostDto>>
{
    public int Limit { get; init; } = 100;
}

public class AttachmentDto
{
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class AcknowledgementDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public DateTime AcknowledgedAt { get; init; }
}

public class CommentDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorAlias { get; init; }
    public string Text { get; init; } = string.Empty;
    public List<string> Mentions { get; init; } = new();
    public List<AttachmentDto> Attachments { get; init; } = new();
    public List<AcknowledgementDto> Acknowledgements { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public class PostDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorAlias { get; init; }
    public string Text { get; init; } = string.Empty;
    public PostType PostType { get; init; }
    public List<string> Mentions { get; init; } = new();
    public List<AttachmentDto> Attachments { get; init; } = new();
    public int? TargetGroupId { get; init; }
    public List<AcknowledgementDto> Acknowledgements { get; init; } = new();
    public List<CommentDto> Comments { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public class GetPostsListQueryHandler : IRequestHandler<GetPostsListQuery, List<PostDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPostsListQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<PostDto>> Handle(GetPostsListQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;
        var currentUser = await _unitOfWork.Users!.GetByIdAsync(currentUserId);

        var currentUserPropertyIds = currentUser?.PropertyAccessList?
            .Where(p => p.IsActive)
            .Select(p => p.Id)
            .ToHashSet() ?? new HashSet<int>();

        var posts = await _unitOfWork.Posts!.GetRecentAsync(request.Limit);

        var visiblePosts = posts.Where(p => IsVisibleToCurrentUser(p, currentUserId, currentUser?.GroupId, currentUserPropertyIds));

        return visiblePosts.Select(p => new PostDto
        {
            Id = p.Id,
            UserId = p.UserId,
            AuthorName = p.AuthorName,
            AuthorAlias = p.AuthorAlias,
            Text = p.Text,
            PostType = p.PostType,
            Mentions = p.Mentions,
            Attachments = p.Attachments.Select(a => new AttachmentDto { Url = a.Url, FileName = a.FileName, ContentType = a.ContentType }).ToList(),
            TargetGroupId = p.TargetGroupId,
            Acknowledgements = p.Acknowledgements.Select(a => new AcknowledgementDto { UserId = a.UserId, UserName = a.UserName, AcknowledgedAt = a.AcknowledgedAt }).ToList(),
            Comments = p.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                AuthorName = c.AuthorName,
                AuthorAlias = c.AuthorAlias,
                Text = c.Text,
                Mentions = c.Mentions,
                Attachments = c.Attachments.Select(a => new AttachmentDto { Url = a.Url, FileName = a.FileName, ContentType = a.ContentType }).ToList(),
                Acknowledgements = c.Acknowledgements.Select(a => new AcknowledgementDto { UserId = a.UserId, UserName = a.UserName, AcknowledgedAt = a.AcknowledgedAt }).ToList(),
                CreatedAt = c.CreatedAt
            }).ToList(),
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    private static bool IsVisibleToCurrentUser(Post post, int currentUserId, int? currentUserGroupId, HashSet<int> currentUserPropertyIds)
    {
        // Authors always see their own posts.
        if (post.UserId == currentUserId)
        {
            return true;
        }

        // Posts targeted at a specific group are only visible to members of that group.
        if (post.TargetGroupId.HasValue)
        {
            return currentUserGroupId.HasValue && currentUserGroupId.Value == post.TargetGroupId.Value;
        }

        // Otherwise, visible only to users who share access to at least one of the poster's properties.
        return post.PosterPropertyIds.Any(currentUserPropertyIds.Contains);
    }
}

