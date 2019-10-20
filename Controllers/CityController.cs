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
    public class CityController : ApiController
    {
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;
        OutputFormat returnString = new OutputFormat();

        // GET: api/Item
        [HttpGet]
        public string GetAllCities()
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<City> cityList = new List<City>();

            try
            {
                var collection = db.GetCollection<City>("City");
                cityList = collection.Find(FilterDefinition<City>.Empty).ToList().OrderBy(x=>x.CityName).ToList();

                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(cityList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);

        }

        // POST: api/City
        [HttpPost]
        public string AddCity(City city)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            CommonController common = new CommonController();

            try
            {
                var collection = db.GetCollection<City>("City");

                var nameFilter = collection.AsQueryable().Where(x => x.CityName.ToLower() == city.CityName.ToLower()).ToList();
                if (nameFilter.Count > 0)
                {
                    throw new Exception("City name already exists");
                }

                if (collection.CountDocuments(FilterDefinition<City>.Empty) == 0)
                {
                    var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "CityID");
                    var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                }

                city.CityID = common.GetNextSequenceValue("CityID", db);
                collection.InsertOne(city);
                returnString.SuccessMessage = "Data inserted successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/City
        [HttpPost]
        public string UpdateCity(City city)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<City>("City");
                var stockCollection = db.GetCollection<Stock>("Stock");
                var usageCollection = db.GetCollection<Usage>("Usage");
                var allocationCollection = db.GetCollection<AllocationDetail>("AllocationDetail");

                var nameFilter = collection.AsQueryable().Where(x => x.CityName.ToLower() == city.CityName.ToLower()).ToList();
                if (nameFilter.Count > 0)
                {
                    throw new Exception("City name already exists");
                }

                FilterDefinition<City> filter = Builders<City>.Filter.Eq(x => x.CityID, city.CityID);
                UpdateDefinition<City> update = Builders<City>.Update
                    .Set(x => x.CityName, city.CityName);
                collection.UpdateOne(filter, update);

                FilterDefinition<Stock> filter1 = Builders<Stock>.Filter.Eq(x => x.City.CityID, city.CityID);
                UpdateDefinition<Stock> update1 = Builders<Stock>.Update
                    .Set(x => x.City, city);
                stockCollection.UpdateMany(filter1, update1);

                FilterDefinition<Usage> filter2 = Builders<Usage>.Filter.Eq(x => x.City.CityID, city.CityID);
                UpdateDefinition<Usage> update2 = Builders<Usage>.Update
                    .Set(x => x.City, city);
                usageCollection.UpdateMany(filter2, update2);

                FilterDefinition<AllocationDetail> filter3 = Builders<AllocationDetail>.Filter.Eq(x => x.City.CityID, city.CityID);
                UpdateDefinition<AllocationDetail> update3 = Builders<AllocationDetail>.Update
                    .Set(x => x.City, city);
                allocationCollection.UpdateMany(filter3, update3);

                returnString.SuccessMessage = "Data updated successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/City
        [HttpGet]
        public string DeleteCity(int cityId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<City>("City");

                FilterDefinition<City> filter = Builders<City>.Filter.Eq(x => x.CityID, cityId);

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
