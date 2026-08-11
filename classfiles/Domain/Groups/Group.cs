using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;

namespace MyWarehouse.Domain.Groups;

/// <summary>
/// A named group of users that posts can be targeted to (e.g. "Front Office", "Management").
/// </summary>
public class Group : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }

    public Group()
    {
    }

    public Group(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
