using MediatR;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Query to get rooms for the room planner
    /// </summary>
    public class GetRoomsByPropertyQuery : IRequest<List<DTOs.GetRoomListDTO>>
    {
        public int UserId { get; set; }
        public List<int> PropertyIds { get; set; } = new List<int>();
        public bool ActiveOnly { get; set; } = true;
    }
}
