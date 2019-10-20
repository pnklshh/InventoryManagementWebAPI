using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventoryManagementWebAPI.Models
{
    public class Item
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        public int ItemID { get; set; }
        [BsonElement]
        public string ItemName { get; set; }
        [BsonElement]
        public Group ItemGroup { get; set; }
    }
}