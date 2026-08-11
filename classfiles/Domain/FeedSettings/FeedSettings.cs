using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.SystemSettings;

/// <summary>
/// Per-user configurable settings for the "Team Feed" (e.g. how many recent posts to show).
/// Stored in the shared "SystemSettings" collection alongside other setting types, discriminated by
/// <see cref="SettingType"/> so the collection can be reused for future system-wide settings.
/// </summary>
public class FeedSettings : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }

    /// <summary>
    /// Discriminator identifying which kind of setting this document represents.
    /// Always "TeamFeed" for this entity; lets the shared SystemSettings collection
    /// host other setting types in the future without collisions.
    /// </summary>
    public string SettingType { get; set; } = SettingTypes.TeamFeed;

    public int UserId { get; set; }

    public int MaxPostsToShow { get; set; } = 50;

    // Audit fields
    public DateTime UpdatedAt { get; set; }

    public FeedSettings()
    {
    }

    public FeedSettings(int userId, int maxPostsToShow)
    {
        UserId = userId;
        MaxPostsToShow = maxPostsToShow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Known discriminator values for documents stored in the shared SystemSettings collection.
/// </summary>
public static class SettingTypes
{
    public const string TeamFeed = "TeamFeed";
}
