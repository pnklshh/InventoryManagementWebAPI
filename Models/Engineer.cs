using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace InventoryManagementWebAPI.Models
{
    public class Engineer
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        public int EngineerID { get; set; }
        [BsonElement]
        public string EngineerName { get; set; }
        [BsonElement]
        public string EmailID { get; set; }
        [BsonElement]
        public string Password { get; set; }
        [BsonElement]
        public string Role { get; set; }
    }
}