using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using InventoryManagementWebAPI.DAL;
using InventoryManagementWebAPI.Models;
using Newtonsoft.Json;

namespace InventoryManagementWebAPI.Controllers
{
    public class ItemController : ApiController
    {
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;
        OutputFormat returnString = new OutputFormat();

        // GET: api/Item
        [HttpGet]
        public string GetAllItems()
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<Item> itemList = new List<Item>();

            try
            {
                var collection = db.GetCollection<Item>("Item");
                itemList = collection.Find(FilterDefinition<Item>.Empty).ToList().OrderBy(x=>x.ItemName).ToList();

                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(itemList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);

        }

        // POST: api/Item
        [HttpPost]
        public string AddItem(Item item)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            CommonController common = new CommonController();

            try
            {
                var collection = db.GetCollection<Item>("Item");

                var nameFilter = collection.AsQueryable().Where(x => x.ItemName.ToLower() == item.ItemName.ToLower()).ToList();
                if(nameFilter.Count > 0)
                {
                    throw new Exception("Item name already exists");
                }

                if (collection.CountDocuments(FilterDefinition<Item>.Empty) == 0)
                {
                    var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "ItemID");
                    var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                }

                item.ItemID = common.GetNextSequenceValue("ItemID", db);
                collection.InsertOne(item);
                returnString.SuccessMessage = "Data inserted successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Item
        [HttpPost]
        public string UpdateItem(Item item)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Item>("Item");
                var groupColl = db.GetCollection<Group>("Group");
                var usageCollection = db.GetCollection<Usage>("Usage");
                var allocationCollection = db.GetCollection<AllocationDetail>("AllocationDetail");
                var stockCollection = db.GetCollection<Stock>("Stock");
                //var nameFilter = collection.AsQueryable().Where(x => x.ItemName.ToLower() == item.ItemName.ToLower()).ToList();
                //if (nameFilter.Count > 0)
                //{
                //    throw new Exception("Item name already exists");
                //}
                Group group = groupColl.AsQueryable().Where(x => x.GroupName == item.ItemGroup.GroupName).FirstOrDefault();
                FilterDefinition<Item> filter = Builders<Item>.Filter.Eq(x => x.ItemID, item.ItemID);
                UpdateDefinition<Item> update = Builders<Item>.Update
                    .Set(x => x.ItemName, item.ItemName)
                    .Set(x => x.ItemGroup, group);
                collection.UpdateOne(filter, update);

                FilterDefinition<Stock> filter1 = Builders<Stock>.Filter.Eq(x => x.Item.ItemID, item.ItemID);
                UpdateDefinition<Stock> update1 = Builders<Stock>.Update
                    .Set(x => x.Item, item);
                stockCollection.UpdateMany(filter1, update1);

                FilterDefinition<AllocationDetail> filter2 = Builders<AllocationDetail>.Filter.Eq(x => x.Item.ItemID, item.ItemID);
                UpdateDefinition<AllocationDetail> update2 = Builders<AllocationDetail>.Update
                    .Set(x => x.Item, item);
                allocationCollection.UpdateMany(filter2, update2);

                FilterDefinition<Usage> filter3 = Builders<Usage>.Filter.Eq(x => x.Item.ItemID, item.ItemID);
                UpdateDefinition<Usage> update3 = Builders<Usage>.Update
                    .Set(x => x.Item, item);
                usageCollection.UpdateMany(filter3, update3);

                returnString.SuccessMessage = "Data updated successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Item
        [HttpGet]
        public string DeleteItem(int itemId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Item>("Item");

                FilterDefinition<Item> filter = Builders<Item>.Filter.Eq(x => x.ItemID, itemId);

                collection.DeleteOne(filter);

                returnString.SuccessMessage = "Data deleted successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }
    }
}
