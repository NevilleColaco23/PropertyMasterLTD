using System.Text.RegularExpressions;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Exceptions;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Domain.Posts;

namespace MyWarehouse.Application.Posts.AddComment;

public class AddCommentCommand : IRequest<int>
{
    public int PostId { get; init; }
    public string AuthorName { get; init; } = null!;
    public string Text { get; init; } = null!;
    public List<AttachmentDto> Attachments { get; init; } = new();

    public record AttachmentDto
    {
        public string Url { get; init; } = null!;
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
    }
}

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public const int TextMaxLength = 280;

    public AddCommentCommandValidator()
    {
        RuleFor(x => x.AuthorName).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(TextMaxLength);
    }
}

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, int>
{
    private static readonly Regex MentionRegex = new(@"@(\w+)", RegexOptions.Compiled);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Posts!.GetByIdAsync(request.PostId);
        if (post == null)
        {
            throw new EntityNotFoundException(nameof(Post), request.PostId);
        }

        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;
        var currentUser = await _unitOfWork.Users!.GetByIdAsync(currentUserId);
        var trimmedText = request.Text.Trim();

        var nextCommentId = post.Comments.Count > 0 ? post.Comments.Max(c => c.Id) + 1 : 1;

        var comment = new PostComment
        {
            Id = nextCommentId,
            UserId = currentUserId,
            AuthorName = request.AuthorName.Trim(),
            AuthorAlias = currentUser?.Alias,
            Text = trimmedText,
            Mentions = MentionRegex.Matches(trimmedText).Select(m => m.Groups[1].Value).Distinct().ToList(),
            Attachments = request.Attachments
                .Select(a => new PostAttachment { Url = a.Url, FileName = a.FileName, ContentType = a.ContentType })
                .ToList(),
            CreatedAt = DateTime.UtcNow
        };

        post.Comments.Add(comment);
        await _unitOfWork.Posts!.Update(post, cancellationToken);

        return comment.Id;
    }
}
