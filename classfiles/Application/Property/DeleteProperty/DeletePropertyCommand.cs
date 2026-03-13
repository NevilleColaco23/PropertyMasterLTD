using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;

namespace MyWarehouse.Application.Property.DeleteProperty;

public class DeletePropertyCommand : IRequest<Unit>
{
    public int Id { get; init; }
}

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeletePropertyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(request.Id);

        if (property == null)
        {
            throw new KeyNotFoundException($"Property with ID {request.Id} not found.");
        }

        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        // Get all rooms associated with this property
        var rooms = await _unitOfWork.Rooms.GetRoomsByPropertyIdAsync(request.Id);

        // Archive the property to PropertyAudit collection
        var propertyAudit = new Domain.Property.PropertyAudit
        {
            Id = property.Id,
            Name = property.Name,
            Active = property.Active,
            CompanyLogoURL = property.CompanyLogoURL,
            PropertyCode = property.PropertyCode,
            CreatedAt = property.CreatedAt,
            CreatedBy = property.CreatedBy,
            UpdatedAt = property.UpdatedAt,
            UpdatedBy = property.UpdatedBy,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = currentUserId,
            DeleteReason = "Property deleted by user",
            Rooms = property.Rooms.Select(r => new Domain.Property.PropertyAudit.PropertyAuditRoom(
                id: r.Id,
                roomCode: r.RoomCode,
                roomName: r.RoomName,
                active: r.Active,
                companyLogoURL: r.CompanyLogoURL
            )).ToList()
        };

        await _unitOfWork.PropertyAudits.Add(propertyAudit, cancellationToken);

        // Archive all rooms to RoomAudit collection
        foreach (var room in rooms)
        {
            var roomAudit = new Domain.Property.RoomAudit
            {
                Id = room.Id,
                PropertyId = property.Id,
                PropertyName = property.Name,
                CompanyLogoURL = room.CompanyLogoURL,
                RoomCode = room.RoomCode,
                RoomName = room.RoomName,
                Active = room.Active,
                CreatedAt = room.CreatedAt,
                CreatedBy = room.CreatedBy,
                UpdatedAt = room.UpdatedAt,
                UpdatedBy = room.UpdatedBy,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = currentUserId,
                DeleteReason = $"Room deleted with property '{property.Name}'"
            };

            await _unitOfWork.RoomAudits.Add(roomAudit, cancellationToken);

            // Soft delete the room - mark as deleted and inactive
            room.IsDeleted = true;
            room.Active = false;
            room.DeletedAt = DateTime.UtcNow;
            room.DeletedBy = currentUserId;

            await _unitOfWork.Rooms.Update(room, cancellationToken);
        }

        // Soft delete the property - mark as deleted and inactive
        var updatedProperty = new Domain.Property.Property(
            name: property.Name,
            isActive: false, // Set to inactive
            rooms: property.Rooms
        )
        {
            Id = property.Id,
            CompanyLogoURL = property.CompanyLogoURL,
            PropertyCode = property.PropertyCode,
            CreatedAt = property.CreatedAt,
            CreatedBy = property.CreatedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = currentUserId,
            IsDeleted = true, // Mark as deleted
            DeletedAt = DateTime.UtcNow,
            DeletedBy = currentUserId
        };

        await _unitOfWork.Properties.Update(updatedProperty, cancellationToken);
        await _unitOfWork.SaveChanges();

        // Log the delete operation to audit trail
        await _auditService.LogDelete(
            "Properties",
            request.Id,
            property,
            currentUserId
        );

        return Unit.Value;
    }
}
