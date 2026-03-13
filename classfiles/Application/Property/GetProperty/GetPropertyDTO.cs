using MyWarehouse.Application.Common.Mapping;

namespace MyWarehouse.Application.Property.GetProperty;

public class GetPropertyDto : IMapFrom<Domain.Property.Property>
{
    public int Id { get; init; }
    public string Name { get; init; }
    public bool Active { get; init; }
    public List<RoomDto> Rooms { get; init; }
    public string CompanyLogoURL { get; init; }
    public string PropertyCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public int CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int? UpdatedBy { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime? DeletedAt { get; init; }
    public int? DeletedBy { get; init; }

    public GetPropertyDto() { }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Domain.Property.Property, GetPropertyDto>()
            .ForMember(dest => dest.CompanyLogoURL, opt => opt.MapFrom(src => src.CompanyLogoURL))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.PropertyCode))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
            .ForMember(dest => dest.DeletedBy, opt => opt.MapFrom(src => src.DeletedBy))
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms));

        profile.CreateMap<Domain.Property.Property.Room, RoomDto>();
    }
}

public class RoomDto
{
    public int Id { get; set; }
    public string CompanyLogoURL { get; set; }
    public string RoomCode { get; set; }
    public string RoomName { get; set; }
    public bool Active { get; set; }
}
