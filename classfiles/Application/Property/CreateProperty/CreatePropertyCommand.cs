using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Application.Common.Audit;

namespace MyWarehouse.Application.Property.CreateProperty;

public class CreatePropertyCommand : IRequest<int>
{
    public CreatePropertyCommand(bool isActive)
    {
        IsActive = isActive;
    }

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

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreatePropertyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = int.TryParse(_currentUserService.UserId, out var userId) ? userId : 0;

        // Validation: At least one room is required
        if (request.Rooms == null || !request.Rooms.Any())
        {
            throw new InvalidOperationException("At least one room must be added to the property.");
        }

        // Verify Rooms repository is available
        if (_unitOfWork.Rooms == null)
        {
            throw new InvalidOperationException("Rooms repository is not available in UnitOfWork.");
        }

        // First, create a temporary property to get the property ID
        var tempRooms = request.Rooms.Select(r => new Domain.Property.Property.Room(
            roomCode: r.RoomCode,
            roomName: r.RoomName,
            isActive: r.Active,
            id: 0, // Temporary ID, will be updated
            companyLogoURL: r.CompanyLogoURL
        )).ToList();

        var property = new Domain.Property.Property(
            name: request.Name.Trim(),
            isActive: request.IsActive,
            rooms: tempRooms
        )
        {
            CompanyLogoURL = request.CompanyLogoURL ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        await _unitOfWork.Properties.Add(property, cancellationToken);
        await _unitOfWork.SaveChanges();

        // Now save rooms to separate collection and get their IDs
        var savedRooms = new List<Domain.Property.Room>();
        foreach (var roomDto in request.Rooms)
        {
            var room = new Domain.Property.Room(
                propertyId: property.Id,
                roomCode: roomDto.RoomCode,
                roomName: roomDto.RoomName,
                isActive: roomDto.Active,
                companyLogoURL: roomDto.CompanyLogoURL
            )
            {
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            var addedRoom = await _unitOfWork.Rooms.Add(room, cancellationToken);

            if (addedRoom == null)
            {
                throw new InvalidOperationException($"Failed to add room '{roomDto.RoomName}' to collection.");
            }

            savedRooms.Add(addedRoom);
        }

        await _unitOfWork.SaveChanges();

        // Update property with correct room IDs from the separate collection
        var roomsWithCorrectIds = savedRooms.Select(r => new Domain.Property.Property.Room(
            roomCode: r.RoomCode,
            roomName: r.RoomName,
            isActive: r.Active,
            id: r.Id, // Use the ID from the separate Room collection
            companyLogoURL: r.CompanyLogoURL
        )).ToList();

        var updatedProperty = new Domain.Property.Property(
            name: property.Name,
            isActive: property.Active,
            rooms: roomsWithCorrectIds
        )
        {
            Id = property.Id,
            CompanyLogoURL = property.CompanyLogoURL,
            PropertyCode = property.PropertyCode,
            CreatedAt = property.CreatedAt,
            CreatedBy = property.CreatedBy
        };

        await _unitOfWork.Properties.Update(updatedProperty, cancellationToken);
        await _unitOfWork.SaveChanges();

        // Log the create operation to audit trail
        await _auditService.LogCreate(
            "Properties",
            property.Id,
            updatedProperty,
            currentUserId
        );

        return property.Id;
    }
}
