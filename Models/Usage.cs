using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace InventoryManagementWebAPI.Models
{
    public class Usage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        public int UsageID { get; set; }
        [BsonElement]
        public Engineer Engineer { get; set; }
        [BsonElement]
        public Item Item { get; set; }
        [BsonElement]
        public int QuantityUsed { get; set; }
        [BsonElement]
        public bool Warranty { get; set; }
        [BsonElement]
        public int BillNumber { get; set; }
        [BsonElement]
        public int Amount { get; set; }
        [BsonElement]
        public int ServiceCharge { get; set; }
        [BsonElement]
        public DateTime UsageDate { get; set; }
        [BsonElement]
        public string Remarks { get; set; }
        [BsonElement]
        public string PartyName { get; set; }
        [BsonElement]
        public City City { get; set; }
    }
}