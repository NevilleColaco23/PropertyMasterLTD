using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace testAngularAPI.Server.Model
{
    [BsonIgnoreExtraElements]
    public class Property
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        public bool Active { get; protected set; }


        [BsonElement("Rooms")]
        public List<Room> Rooms { get; set; }

        public Property(string name, bool isActive, List<Room> rooms)
        {
            Name = name;
            Active = isActive;
            Rooms = rooms;
        }
    }

    [BsonIgnoreExtraElements]
    public class Room
    {
        [BsonId]
        public string Id { get; set; }
        public string RoomCode { get; protected set; }
        public string RoomName { get; protected set; }
        public bool Active { get; protected set; }

        public Room(string roomCode, string roomName, bool isActive, string id)
        {
            RoomCode = roomCode;
            RoomName = roomName;
            Active = isActive;
            Id = id;
        }
    }
}
