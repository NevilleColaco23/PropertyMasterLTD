using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;

namespace MyWarehouse.Application.Property.UpdateProperty;

public class UpdatePropertyCommand : IRequest<Unit>
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public bool IsActive { get; init; }
    public string CompanyLogoURL { get; init; } = string.Empty;
    public List<RoomDto> Rooms { get; init; } = new();
}

public class RoomDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string CompanyLogoURL { get; set; } = string.Empty;
}

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdatePropertyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        // Validation: At least one room is required
        if (request.Rooms == null || !request.Rooms.Any())
        {
            throw new InvalidOperationException("At least one room must be added to the property.");
        }

        // Get old value for audit trail
        var oldProperty = await _unitOfWork.Properties.GetByIdAsync(request.Id);

        if (oldProperty == null)
        {
            throw new KeyNotFoundException($"Property with ID {request.Id} not found.");
        }

        // Map RoomDto to Domain.Property.Room
        var rooms = request.Rooms.Select(r => new Domain.Property.Property.Room(
            roomCode: r.RoomCode,
            roomName: r.RoomName,
            isActive: r.Active,
            id: r.Id,
            companyLogoURL: r.CompanyLogoURL
        )).ToList();

        // Create new property instance with updated values
        var updatedProperty = new Domain.Property.Property(
            name: request.Name.Trim(),
            isActive: request.IsActive,
            rooms: rooms
        )
        {
            Id = request.Id,
            CompanyLogoURL = request.CompanyLogoURL ?? string.Empty,
            PropertyCode = oldProperty.PropertyCode,
            CreatedAt = oldProperty.CreatedAt,
            CreatedBy = oldProperty.CreatedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = currentUserId
        };

        await _unitOfWork.Properties.Update(updatedProperty, cancellationToken);
        await _unitOfWork.SaveChanges();

        // Log the update operation to audit trail
        await _auditService.LogUpdate(
            "Properties",
            request.Id,
            oldProperty,
            updatedProperty,
            currentUserId
        );

        return Unit.Value;
    }
}
