using MyWarehouse.Domain.Common;
using MongoDB.Bson;

namespace MyWarehouse.Domain.Other_Entities
{
    public class KeyCounter : IEntity<ObjectId>
    {
        public ObjectId Id { get; set; }
        public string EntityName { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
        public string SetValue { get; set; }
        public int Sequence { get; set; }
    }
}
