using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Libmongocrypt;
using MyWarehouse.Application.Common.Mapping;
using MyWarehouse.Domain.Users;

namespace MyWarehouse.Application.Users.GetUsersByUserid
{
    public class GetUsersbyUserIdDto : IMapFrom<Domain.Users.Users>
    {
        public int Id { get; init; }

        public string UserName { get; protected set; }
        public string NormalizedUserName { get; protected set; }

        public string Email { get; init; }
        public string NormalizedEmail { get; init; }
        public bool EmailConfirmed { get; init; }
        public string PasswordHash { get; init; }
        public string SecurityStamp { get; init; }
        public string ConcurrencyStamp { get; init; }
        public int? PhoneNumber { get; init; }
        public bool PhoneNumberConfirmed { get; init; }
        public bool TwoFactorEnabled { get; init; }
        public string LockoutEnd { get; init; }
        public bool LockoutEnabled { get; init; }
        public int AccessFailedCount { get; init; }
        public int Version { get; init; }
        public DateTime CreatedOn { get; init; }
        public List<PropertyAccessList> PropertyAccessList { get; init; }
        public List<PropertyList> PropertyList { get; init; }


        public GetUsersbyUserIdDto() { } //Error1

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Users.Users, GetUsersbyUserIdDto>();
        }
    }

    public class PropertyList
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public List<Rooms> Rooms { get; init; }
    }
    public class Rooms
    {
        public string RoomName { get; init; }
        public string RoomCode { get; init; }
        public string Id { get; init; }
        public bool Active { get; init; }
    }
}