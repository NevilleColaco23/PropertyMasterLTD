namespace MyWarehouse.Domain.Common;

public abstract class MyEntity : IEntity<int>, ISoftDeletable, IAudited
{
    public int Id { get; set; }

    public string CreatedBy { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public string? LastModifiedBy { get; private set; }

    public DateTime? LastModifiedAt { get; private set; }

    public string? DeletedBy { get; private set; }
    
    public DateTime? DeletedAt { get; private set; }
}
