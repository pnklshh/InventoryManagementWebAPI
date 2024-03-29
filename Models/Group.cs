﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventoryManagementWebAPI.Models
{
    public class Group
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        public int GroupID { get; set; }
        [BsonElement]
        public string GroupName { get; set; }
    }
}