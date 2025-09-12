using MongoDB.Bson;
using System;

namespace MyWarehouse.Application.Common.Messages
{
    public class GetSystemMessagesDTO
    {
        public ObjectId _id { get; set; }
        public string message { get; set; }
        public string priority { get; set; }
        public DateTime timeFrom { get; set; }
        public DateTime timeTo { get; set; }
    }
}
