using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;

namespace InventoryManagementWebAPI.DAL
{
    public class MongoConnection
    {
        public IMongoDatabase database;
        public MongoConnection()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            database = client.GetDatabase("InventoryDB");
        }
        
    }
}