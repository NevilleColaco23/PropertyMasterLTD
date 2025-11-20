using MongoDB.Bson.Serialization.Attributes;
using MyWarehouse.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWarehouse.Domain.AccessLog
{
    public class AccessLog : IEntity<int>
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("Log")]
        public string Log { get; protected set; }
        public string Action { get; protected set; }
        public string Details { get; protected set; }

        [BsonElement("User")]
        public int User { get; set; }
        public DateTime TimeStamp { get; set; }

        public AccessLog(int id, string log, int user,DateTime time, string action, string details)
        {
            Id = id;
            Log = log;
            User = user;
            TimeStamp = time;
            Action = action;
            Details = details;
        }
    }
}
