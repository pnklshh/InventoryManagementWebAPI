using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using InventoryManagementWebAPI.Models;
using InventoryManagementWebAPI.DAL;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Globalization;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using System.Configuration;

namespace InventoryManagementWebAPI.Controllers
{
    //[EnableCors(origins:"*",headers:"*",methods:"*")]
    public class StockController : ApiController
    {
        public StockController()
        {

        }

        //Connection db = new Connection();
        //MySqlConnection connection;
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;

        OutputFormat returnString = new OutputFormat();
        
        // GET: api/Stock
        [HttpGet]
        public string GetAllStocks(string itemname, string cityname, string challannumber)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<Stock> stockList = new List<Stock>();

            try
            {
                var collection = db.GetCollection<Stock>("Stock");
                var filter = FilterDefinition<Stock>.Empty;
                if (itemname != null)
                    filter &= Builders<Stock>.Filter.Eq(a => a.Item.ItemName, itemname);
                if(cityname != null)
                    filter &= Builders<Stock>.Filter.Eq(a => a.City.CityName, cityname);
                if(challannumber != null)
                    filter &= Builders<Stock>.Filter.Eq(a => a.ChallanNumber, challannumber);
                
                stockList = collection.Find(filter).ToList();
                //stockList = collection.Find(FilterDefinition<Stock>.Empty).ToList();

                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(stockList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
            
        }


        // POST: api/Stock
        [HttpPost]
        public string AddStock(Stock stock)
        {
            //connection = db.connection;
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            CommonController common = new CommonController();
            AppConstants appConstants = new AppConstants();
            //string query = "insert into stock(item_name,quantity,defective,dead,date,place,challan_no) values('" + stock.ItemName + "'," + stock.Quantity + ",'" + stock.Defective + "','" + stock.Dead + "','" + stock.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "','" + stock.Place + "','" + stock.ChallanNumber + "')";
            //List<Stock> stockList = new List<Stock>();

            try
            {
                //connection.Open();

                //MySqlCommand cmd = new MySqlCommand(query, connection);
                //int rowsAffected = cmd.ExecuteNonQuery();

                //cmd.Dispose();
                //connection.Close();
                //var date = JsonConvert.DeserializeObject("{ \"$date\" : "+ stock.Date.ToString() +" }");

                var collection = db.GetCollection<Stock>("Stock");
                
                if (collection.CountDocuments(FilterDefinition<Stock>.Empty) == 0)
                {
                    var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "StockID");
                    var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                }

                stock.StockID = common.GetNextSequenceValue("StockID", db);

                if(String.IsNullOrEmpty(stock.ChallanNumber))
                {
                    stock.ChallanNumber = ConfigurationManager.AppSettings["Default_ChallanNumber"];
                }

                collection.InsertOne(stock);
                returnString.SuccessMessage = "Data inserted successfully";
                
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Stock
        [HttpPost]
        public string UpdateStock(Stock stock)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";
            AppConstants appConstants = new AppConstants();

            try
            {
                var collection = db.GetCollection<Stock>("Stock");

                if (String.IsNullOrEmpty(stock.ChallanNumber))
                {
                    stock.ChallanNumber = ConfigurationManager.AppSettings["Default_ChallanNumber"];
                }

                FilterDefinition<Stock> filter = Builders<Stock>.Filter.Eq(x => x.StockID, stock.StockID);
                UpdateDefinition<Stock> update = Builders<Stock>.Update
                    .Set(x => x.Item, stock.Item)
                    .Set(x => x.Quantity, stock.Quantity)
                    .Set(x => x.Defective, stock.Defective)
                    .Set(x => x.Dead, stock.Dead)
                    .Set(x => x.ChallanNumber, stock.ChallanNumber)
                    .Set(x => x.City, stock.City)
                    .Set(x => x.Date, stock.Date);

                collection.UpdateOne(filter, update);

                returnString.SuccessMessage = "Data updated successfully";
                
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }
        
        // POST: api/Stock
        [HttpGet]
        public string DeleteStock(int stockId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Stock>("Stock");

                FilterDefinition<Stock> filter = Builders<Stock>.Filter.Eq(x => x.StockID, stockId);
                
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
