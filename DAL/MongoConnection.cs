using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using System.Configuration;

namespace InventoryManagementWebAPI.DAL
{
    public class MongoConnection
    {
        public IMongoDatabase database;
        public MongoConnection()
        {
            //string connectionString = "mongodb://localhost:27017";
            //string connectionString = "mongodb+srv://admin:admin123@testcluster.vu5lj.mongodb.net/test";
            string connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];
            MongoClient client = new MongoClient(connectionString);
            database = client.GetDatabase("InventoryDB");
        }
        
    }
}