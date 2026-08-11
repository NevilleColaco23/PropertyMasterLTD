using System.Text.RegularExpressions;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.Posts;

namespace MyWarehouse.Application.Posts.CreatePost;

public class CreatePostCommand : IRequest<int>
{
    public string AuthorName { get; init; } = null!;
    public string Text { get; init; } = null!;
    public PostType PostType { get; init; } = PostType.General;
    public int? TargetGroupId { get; init; }
    public List<AttachmentDto> Attachments { get; init; } = new();

    public record AttachmentDto
    {
        public string Url { get; init; } = null!;
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
    }
}

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public const int TextMaxLength = 280;

    public CreatePostCommandValidator()
    {
        RuleFor(x => x.AuthorName)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(TextMaxLength);
    }
}

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private static readonly Regex MentionRegex = new(@"@(\w+)", RegexOptions.Compiled);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;
        var currentUser = await _unitOfWork.Users!.GetByIdAsync(currentUserId);

        var trimmedText = request.Text.Trim();

        var post = new Post(
            userId: currentUserId,
            authorName: request.AuthorName.Trim(),
            text: trimmedText)
        {
            AuthorAlias = currentUser?.Alias,
            PostType = request.PostType,
            TargetGroupId = request.TargetGroupId,
            Mentions = ExtractMentions(trimmedText),
            Attachments = request.Attachments
                .Select(a => new PostAttachment { Url = a.Url, FileName = a.FileName, ContentType = a.ContentType })
                .ToList(),
            PosterPropertyIds = currentUser?.PropertyAccessList?
                .Where(p => p.IsActive)
                .Select(p => p.Id)
                .Distinct()
                .ToList() ?? new List<int>(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        var added = await _unitOfWork.Posts!.Add(post, cancellationToken);

        return added.Id;
    }

    private static List<string> ExtractMentions(string text)
        => MentionRegex.Matches(text).Select(m => m.Groups[1].Value).Distinct().ToList();
}
