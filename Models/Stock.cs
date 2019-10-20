using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementWebAPI.Models
{
    public class Stock
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        public int StockID { get; set; }
        [BsonElement]
        public Item Item { get; set; }
        [BsonElement]
        public int Quantity { get; set; }
        [BsonElement]
        public int Defective { get; set; }
        [BsonElement]
        public int Dead { get; set; }
        [BsonElement]
        public DateTime Date { get; set; }
        [BsonElement]
        public City City { get; set; }
        [BsonElement]
        public string ChallanNumber { get; set; }
    }
}