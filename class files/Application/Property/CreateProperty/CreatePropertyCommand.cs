using MyWarehouse.Application.Common.Dependencies.DataAccess;

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
}

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePropertyCommandHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = new Domain.Property.Property(
            name: request.Name.Trim(),
            isActive: true,new List<Domain.Property.Property.Room>()
            );

        _unitOfWork.Properties?.Add(property);
        await _unitOfWork.SaveChanges();

        return property.Id;
    }
}