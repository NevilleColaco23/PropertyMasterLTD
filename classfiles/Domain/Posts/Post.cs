using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.Posts;

public enum PostType
{
    General = 0,
    Important = 1,
    Announcement = 2,
    Question = 3
}

public class PostAttachment
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class Acknowledgement
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime AcknowledgedAt { get; set; }
}

public class PostComment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAlias { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Mentions { get; set; } = new();
    public List<PostAttachment> Attachments { get; set; } = new();
    public List<Acknowledgement> Acknowledgements { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class Post : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }

    public int UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// The alias the author chose to post under (see Users.Alias), used to @mention them.
    /// </summary>
    public string? AuthorAlias { get; set; }

    public string Text { get; set; } = string.Empty;
    public PostType PostType { get; set; } = PostType.General;

    /// <summary>
    /// Aliases of other users @mentioned in this post's text.
    /// </summary>
    public List<string> Mentions { get; set; } = new();

    public List<PostAttachment> Attachments { get; set; } = new();

    /// <summary>
    /// If set, this post is only visible to users belonging to this Group (see Domain.Groups.Group).
    /// If null, visibility falls back to property-access matching (see PosterPropertyIds).
    /// </summary>
    public int? TargetGroupId { get; set; }

    /// <summary>
    /// Snapshot, at creation time, of the property IDs the author had access to.
    /// Used (when TargetGroupId is null) so the post is only visible to users who share
    /// access to at least one of these properties.
    /// </summary>
    public List<int> PosterPropertyIds { get; set; } = new();

    public List<Acknowledgement> Acknowledgements { get; set; } = new();
    public List<PostComment> Comments { get; set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }

    public Post()
    {
    }

    public Post(int userId, string authorName, string text)
    {
        UserId = userId;
        AuthorName = authorName;
        Text = text;
    }
}
