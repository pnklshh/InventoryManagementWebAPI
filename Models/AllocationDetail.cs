using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace InventoryManagementWebAPI.Models
{
    public class AllocationDetail
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        public int AllocationID { get; set; }
        [BsonElement]
        public Engineer Engineer { get; set; }
        [BsonElement]
        public Item Item { get; set; }
        [BsonElement]
        public int QuantityAllocated { get; set; }
        [BsonElement]
        public City City  { get; set; }
        [BsonElement]
        //[BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime AllocationDate { get; set; }
    }
}