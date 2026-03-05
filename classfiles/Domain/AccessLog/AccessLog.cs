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
        private string user;
        private DateTime? time;

        [BsonId]
        public int Id { get; set; }

        [BsonElement("Log")]
        public string Log { get; protected set; }
        public string Action { get; protected set; }
        public string Details { get; protected set; }
        public string Source { get; protected set; }
        public string? IpAddress { get; protected set; }

        [BsonElement("User")]
        public int UserID { get; set; }
        public DateTime TimeStamp { get; set; }

        public AccessLog(int id, string log, int user, DateTime time, string action, string details, string source = "Direct", string? ipAddress = null)
        {
            Id = id;
            Log = log;
            UserID = user;
            TimeStamp = time;
            Action = action;
            Details = details;
            Source = source;
            IpAddress = ipAddress;
        }

        public AccessLog(int id, string log, string user, DateTime? time, string action, string details, string source = "Direct", string? ipAddress = null)
        {
            Id = id;
            Log = log;
            this.user = user;
            this.time = time;
            Action = action;
            Details = details;
            Source = source;
            IpAddress = ipAddress;
        }
    }
}
