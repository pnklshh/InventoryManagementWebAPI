using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InventoryManagementWebAPI.DAL;
using InventoryManagementWebAPI.Models;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Web.Http;
using System.Data.Entity;
using System.Configuration;

namespace InventoryManagementWebAPI.Controllers
{
    public class EngineerController : ApiController
    {
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;
        OutputFormat returnString = new OutputFormat();

        // GET: api/Engineer
        [HttpGet]
        public string GetAllEngineers()
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<Engineer> engineerList = new List<Engineer>();

            try
            {
                var collection = db.GetCollection<Engineer>("Engineer");
                engineerList = collection.Find(FilterDefinition<Engineer>.Empty).ToList().OrderBy(x=>x.EngineerName).ToList();

                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(engineerList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Engineer
        [HttpPost]
        public string AddEngineer(Engineer engineer)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            CommonController common = new CommonController();

            try
            {
                var collection = db.GetCollection<Engineer>("Engineer");

                var duplicateFilter = collection.AsQueryable().Where(x => x.EngineerName.ToLower() == engineer.EngineerName.ToLower() && x.EmailID.ToLower() == engineer.EmailID.ToLower() && x.Role == engineer.Role).ToList();
                var duplicateEmail = collection.AsQueryable().Where(x => x.EmailID.ToLower() == engineer.EmailID.ToLower()).ToList();

                if (duplicateFilter.Count > 0)
                {
                    throw new Exception("Engineer already present in database");
                }
                if (duplicateEmail.Count > 0)
                {
                    throw new Exception("EmailID already taken by another user");
                }

                if (collection.CountDocuments(FilterDefinition<Engineer>.Empty) == 0)
                {
                    var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "EngineerID");
                    var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                }

                engineer.EngineerID = common.GetNextSequenceValue("EngineerID", db);
                collection.InsertOne(engineer);
                returnString.SuccessMessage = "Data inserted successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Engineer
        [HttpPost]
        public string UpdateEngineer(Engineer engineer)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Engineer>("Engineer");
                var usageCollection = db.GetCollection<Usage>("Usage");
                var allocationCollection = db.GetCollection<AllocationDetail>("AllocationDetail");

                var duplicateFilter = collection.AsQueryable().Where(x => x.EngineerName.ToLower() == engineer.EngineerName.ToLower() && x.EmailID.ToLower() == engineer.EmailID.ToLower() && x.Role == engineer.Role).ToList();
                var duplicateEmail = collection.AsQueryable().Where(x => x.EmailID.ToLower() == engineer.EmailID.ToLower()).ToList();

                if (duplicateFilter.Count > 0)
                {
                    throw new Exception("Engineer already present in database");
                }
                if (duplicateEmail.Count > 0)
                {
                    throw new Exception("EmailID already taken by another user");
                }

                FilterDefinition<Engineer> filter = Builders<Engineer>.Filter.Eq(x => x.EngineerID, engineer.EngineerID);
                UpdateDefinition<Engineer> update = Builders<Engineer>.Update
                    .Set(x => x.EngineerName, engineer.EngineerName)
                    .Set(x => x.EmailID, engineer.EmailID)
                    .Set(x => x.Password, engineer.Password)
                    .Set(x => x.Role, engineer.Role);
                collection.UpdateOne(filter, update);

                FilterDefinition<AllocationDetail> filter1 = Builders<AllocationDetail>.Filter.Eq(x => x.Engineer.EngineerID, engineer.EngineerID);
                UpdateDefinition<AllocationDetail> update1 = Builders<AllocationDetail>.Update
                    .Set(x => x.Engineer, engineer);
                allocationCollection.UpdateMany(filter1, update1);

                FilterDefinition<Usage> filter2 = Builders<Usage>.Filter.Eq(x => x.Engineer.EngineerID, engineer.EngineerID);
                UpdateDefinition<Usage> update2 = Builders<Usage>.Update
                    .Set(x => x.Engineer, engineer);
                usageCollection.UpdateMany(filter2, update2);

                returnString.SuccessMessage = "Data updated successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Engineer
        [HttpGet]
        public string DeleteEngineer(int engineerId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Engineer>("Engineer");

                FilterDefinition<Engineer> filter = Builders<Engineer>.Filter.Eq(x => x.EngineerID, engineerId);

                collection.DeleteOne(filter);

                returnString.SuccessMessage = "Data deleted successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }


        // GET: api/Engineer
        [HttpGet]
        public string GetAllocations()
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<AllocationDetail> allocationList = new List<AllocationDetail>();

            try
            {
                var collection = db.GetCollection<AllocationDetail>("AllocationDetail");
                var itemCollection = db.GetCollection<Item>("Item");
                var engineerCollection = db.GetCollection<Engineer>("Engineer");

                allocationList = collection.Find(FilterDefinition<AllocationDetail>.Empty).ToList();
                var itemList = itemCollection.Find(FilterDefinition<Item>.Empty).ToList();
                var engineerList = engineerCollection.Find(FilterDefinition<Engineer>.Empty).ToList();

                foreach (var alloc in allocationList)
                {
                    if (alloc.Engineer == null)
                    {
                        var engineer = new Engineer();
                        engineer.EngineerID = -1;
                        engineer.EngineerName = "Ahmedabad Office";
                        alloc.Engineer = engineer;
                    }
                }

                //allocationList = (from alloc in allocationDetail
                //                  join item in itemList
                //                  on alloc.Item.ItemID equals item.ItemID
                //                  join engineer in engineerList
                //                  on alloc.Engineer.EngineerID equals engineer.EngineerID
                //                  select new AllocationDetail
                //                  {
                //                      AllocationID = alloc.AllocationID,
                //                      Engineer = engineer,
                //                      Item = item,
                //                      QuantityAllocated = alloc.QuantityAllocated,
                //                      AllocationDate = alloc.AllocationDate
                //                  }).ToList();
                
                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(allocationList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Engineer
        [HttpPost]
        public string AllocateItem(AllocationDetail allocationDetail)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";
            CommonController common = new CommonController();
            AppConstants appConstants = new AppConstants();

            try
            {
                var resultDateTime = new DateTime();
                var stockCollection = db.GetCollection<Stock>("Stock");
                var collection = db.GetCollection<AllocationDetail>("AllocationDetail");
            
                //allocationDetail.AllocationDate = new DateTime(allocationDetail.AllocationDate.Year, allocationDetail.AllocationDate.Month, allocationDetail.AllocationDate.Day, 0, 0, 0, 0, DateTimeKind.Utc);

                var allocationResult=collection.Find(x => x.Item == allocationDetail.Item && x.Engineer == allocationDetail.Engineer && x.AllocationDate >= allocationDetail.AllocationDate.Date && x.AllocationDate < allocationDetail.AllocationDate.Date.AddDays(1)).ToList(); 

                var result = stockCollection.AsQueryable().Where(x => x.Item == allocationDetail.Item && x.City == allocationDetail.City).OrderBy(x => x.Date).ToList();//.Select(n => new { Item=n.Item, Quantity=n.Quantity, Defective=n.Defective, Dead=n.Dead, ChallanNumber=n.ChallanNumber, Date=n.Date, Place=n.Place, assignableQuantity = n.Quantity - n.Defective - n.Dead });
                var resultWithDateFilter = stockCollection.AsQueryable().Where(x => x.Item == allocationDetail.Item && x.City == allocationDetail.City && allocationDetail.AllocationDate.AddDays(1) > x.Date).OrderBy(x => x.Date).ToList();
                if(resultWithDateFilter.Count != 0)
                    resultDateTime = new DateTime(resultWithDateFilter.FirstOrDefault().Date.Year, resultWithDateFilter.FirstOrDefault().Date.Month, resultWithDateFilter.FirstOrDefault().Date.Day, 0, 0, 0, 0, DateTimeKind.Utc);

                int totalQuantity = resultWithDateFilter.Sum(x => x.Quantity);
                int totalDefective = resultWithDateFilter.Sum(x => x.Defective);
                int totalDead = resultWithDateFilter.Sum(x => x.Dead);

                if (result.Count() == 0)
                {
                    returnString.ErrorMessage = allocationDetail.Item.ItemName + " is not in " + allocationDetail.City.CityName + " stock";
                }
                else if (allocationDetail.AllocationDate < resultDateTime)
                {
                    returnString.ErrorMessage = "Allocation of " + allocationDetail.Item.ItemName + " cannot be done on selected date. Please check the date when item is added to " + allocationDetail.City.CityName + " stock";
                }
                else if (allocationDetail.Engineer != null  && allocationDetail.QuantityAllocated > totalQuantity - totalDefective - totalDead)
                {
                    returnString.ErrorMessage = "Insufficient quantity of " + allocationDetail.Item.ItemName + "s which can be allocated. Please check the stock";
                }
                
                else
                {
                    if (allocationResult.Any())
                    {
                        var filter = Builders<AllocationDetail>.Filter.Eq(x => x.AllocationID, allocationResult.FirstOrDefault().AllocationID);
                        var update = Builders<AllocationDetail>.Update.Set(x => x.QuantityAllocated, allocationResult.FirstOrDefault().QuantityAllocated + allocationDetail.QuantityAllocated);
                        var updatedDocument = collection.FindOneAndUpdate(filter, update);
                    }
                    else
                    {
                        if (collection.CountDocuments(FilterDefinition<AllocationDetail>.Empty) == 0)
                        {
                            var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "AllocationID");
                            var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                        }

                        allocationDetail.AllocationID = common.GetNextSequenceValue("AllocationID", db);
                        collection.InsertOne(allocationDetail);
                    }

                    returnString.SuccessMessage = "Data inserted successfully";

                    while (allocationDetail.QuantityAllocated > 0)
                    {
                        var query = allocationDetail.Engineer != null ? stockCollection.AsQueryable().Where(x => x.Item == allocationDetail.Item && x.City == allocationDetail.City).Select(n => new { StockID = n.StockID, Item = n.Item, Quantity = n.Quantity, Defective = n.Defective, Dead = n.Dead, ChallanNumber = n.ChallanNumber, Date = n.Date, City = n.City, assignableQuantity = n.Quantity - n.Defective - n.Dead }).ToList() : stockCollection.AsQueryable().Where(x => x.Item == allocationDetail.Item && x.City == allocationDetail.City && x.ChallanNumber == ConfigurationManager.AppSettings["Defective_ChallanNumber"]).Select(n => new { StockID = n.StockID, Item = n.Item, Quantity = n.Quantity, Defective = n.Defective, Dead = n.Dead, ChallanNumber = n.ChallanNumber, Date = n.Date, City = n.City, assignableQuantity = n.Defective }).ToList();
                        var allocatedItem = query.Where(x => x.assignableQuantity > 0).OrderBy(x => x.Date).FirstOrDefault();
                        var quantity = allocatedItem.Quantity;
                        var defective = allocatedItem.Defective;
                        var dead = allocatedItem.Dead;
                        var stockId = allocatedItem.StockID;

                        var setQuantity = quantity - defective - dead - allocationDetail.QuantityAllocated;
                        if (setQuantity > 0)
                        {
                            var stockFilter = Builders<Stock>.Filter.Eq(x => x.StockID, stockId);
                            var update = Builders<Stock>.Update.Set(x => x.Quantity, quantity - allocationDetail.QuantityAllocated);
                            var updatedDocument = stockCollection.FindOneAndUpdate(stockFilter, update);
                            allocationDetail.QuantityAllocated = 0;
                        }
                        else
                        {
                            if (setQuantity == 0 || allocationDetail.Engineer == null)
                                allocationDetail.QuantityAllocated = 0;
                            else if (setQuantity < 0 && allocationDetail.Engineer != null)
                                allocationDetail.QuantityAllocated = allocationDetail.QuantityAllocated - (quantity - defective - dead);

                            if ((defective == 0 && dead == 0) || (allocationDetail.Engineer == null))
                                stockCollection.DeleteOne<Stock>(x => x.StockID == stockId);
                            else if (defective > 0 || dead > 0)
                            {
                                var stockFilter = Builders<Stock>.Filter.Eq(x => x.StockID, stockId);
                                var update = Builders<Stock>.Update.Set(x => x.Quantity, defective + dead);
                                var updatedDocument = stockCollection.FindOneAndUpdate(stockFilter, update);
                            }
                        }                        
                    }
                    
                }
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Engineer
        [HttpPost]
        public string FixDefect(AllocationDetail allocation)
        {
            //connection = db.connection;
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            CommonController common = new CommonController();

            try
            {
                var collection = db.GetCollection<Stock>("Stock");
                var allocationCollection = db.GetCollection<AllocationDetail>("AllocationDetail");

                if (collection.CountDocuments(FilterDefinition<Stock>.Empty) == 0)
                {
                    var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "StockID");
                    var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                }
                Stock stock = new Stock();
                stock.StockID = common.GetNextSequenceValue("StockID", db);
                stock.City = allocation.City;
                stock.Date = DateTime.Now;
                stock.Dead = 0;
                stock.Defective = 0;
                stock.Quantity = allocation.QuantityAllocated;
                stock.Item = allocation.Item;
                if (String.IsNullOrEmpty(stock.ChallanNumber))
                {
                    stock.ChallanNumber = ConfigurationManager.AppSettings["Default_ChallanNumber"];
                }

                collection.InsertOne(stock);
                returnString.SuccessMessage = "Data inserted successfully";

                while (allocation.QuantityAllocated > 0)
                {
                    var usedItem = allocationCollection.AsQueryable().Where(x => x.AllocationID == allocation.AllocationID).FirstOrDefault();

                    var quantity = usedItem.QuantityAllocated;

                    var setQuantity = quantity - allocation.QuantityAllocated;
                    if (setQuantity > 0)
                    {
                        var allocationFilter = Builders<AllocationDetail>.Filter.Eq(x => x.AllocationID, usedItem.AllocationID);
                        var update = Builders<AllocationDetail>.Update.Set(x => x.QuantityAllocated, setQuantity);
                        var updatedDocument = allocationCollection.FindOneAndUpdate(allocationFilter, update);
                        allocation.QuantityAllocated = 0;
                    }
                    else
                    {
                        if (setQuantity < 0)
                            allocation.QuantityAllocated = allocation.QuantityAllocated - quantity;
                        else if (setQuantity == 0)
                            allocation.QuantityAllocated = 0;

                        allocationCollection.DeleteOne<AllocationDetail>(x => x.AllocationID == usedItem.AllocationID);
                    }

                }

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Engineer
        [HttpPost]
        public string UpdateAllocation(AllocationDetail allocation)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<AllocationDetail>("AllocationDetail");
                
                FilterDefinition<AllocationDetail> filter = Builders<AllocationDetail>.Filter.Eq(x => x.AllocationID, allocation.AllocationID);
                UpdateDefinition<AllocationDetail> update = Builders<AllocationDetail>.Update
                    .Set(x => x.Item, allocation.Item)
                    .Set(x => x.QuantityAllocated, allocation.QuantityAllocated)
                    .Set(x => x.Engineer, allocation.Engineer)
                    .Set(x => x.City, allocation.City)
                    .Set(x => x.AllocationDate, allocation.AllocationDate);

                collection.UpdateOne(filter, update);

                returnString.SuccessMessage = "Data updated successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // GET: api/Engineer
        [HttpGet]
        public string DeleteAllocation(int allocationId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<AllocationDetail>("AllocationDetail");

                FilterDefinition<AllocationDetail> filter = Builders<AllocationDetail>.Filter.Eq(x => x.AllocationID, allocationId);

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