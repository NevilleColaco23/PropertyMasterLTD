using MyWarehouse.Application.Common.Mapping;

namespace MyWarehouse.Application.Property.GetProperty;

public class GetPropertyDto : IMapFrom<Domain.Property.Property>
{
    public int Id { get; init; }
    public string Name { get; init; }
    public List<RoomDto> Rooms { get; init; }
    public string CompanyLogo { get; init; }

    public GetPropertyDto() { }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Domain.Property.Property, GetPropertyDto>()
            .ForMember(dest => dest.CompanyLogo, opt => opt.MapFrom(src => src.CompanyLogoURL))
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms));

        profile.CreateMap<Domain.Property.Property.Room, RoomDto>();
    }
}

public class RoomDto
{
    public string Id { get; set; }
    public string CompanyLogoURL { get; set; }
    public string RoomCode { get; set; }
    public string RoomName { get; set; }
    public bool Active { get; set; }
}