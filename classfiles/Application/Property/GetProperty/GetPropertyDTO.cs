using MyWarehouse.Application.Common.Mapping;

namespace MyWarehouse.Application.Property.GetProperty;

public class GetPropertyDto : IMapFrom<Domain.Property.Property>
{
    public int Id { get; init; }
    public string Name { get; init; }
    public List<Domain.Property.Property.Room> Rooms { get; init; }
    public string CompanyLogo { get; init; }

    public GetPropertyDto() { } //Error1
    public GetPropertyDto(string name, List<Domain.Property.Property.Room> rooms)
    {
        Name = name;
        Rooms = rooms;
    }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Domain.Property.Property, GetPropertyDto>();
    }
}