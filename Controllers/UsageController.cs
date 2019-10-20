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
using Newtonsoft.Json.Linq;

namespace InventoryManagementWebAPI.Controllers
{
    public class UsageController : ApiController
    {
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;
        OutputFormat returnString = new OutputFormat();

        // GET: api/Usage
        [HttpGet]
        public string GetHistory()
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<Usage> usageList = new List<Usage>();

            try
            {
                var collection = db.GetCollection<Usage>("Usage");
                usageList = collection.Find(FilterDefinition<Usage>.Empty).ToList();

                //var itemCollection = db.GetCollection<Item>("Item");
                //var engineerCollection = db.GetCollection<Engineer>("Engineer");
                //var cityCollection = db.GetCollection<City>("City");

                //var itemList = itemCollection.Find(FilterDefinition<Item>.Empty).ToList();
                //var engineerList = engineerCollection.Find(FilterDefinition<Engineer>.Empty).ToList();
                //var cityList = cityCollection.Find(FilterDefinition<City>.Empty).ToList();

                //usageList = (from usage in usages
                //            join item in itemList
                //            on usage.Item.ItemID equals item.ItemID
                //            join engineer in engineerList
                //            on usage.Engineer.EngineerID equals engineer.EngineerID
                //            join city in cityList
                //            on usage.City.CityID equals city.CityID
                //            select new Usage
                //            {
                //                UsageID = usage.UsageID,
                //                Engineer = engineer,
                //                Item = item,
                //                City = city,
                //                QuantityUsed = usage.QuantityUsed,
                //                UsageDate = usage.UsageDate,
                //                PartyName = usage.PartyName,
                //                BillNumber = usage.BillNumber,
                //                Remarks = usage.Remarks,
                //                Warranty = usage.Warranty
                //            }).ToList();

                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(usageList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Usage
        [HttpPost]
        public string ReportUsage(Usage usageDetail)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";
            CommonController common = new CommonController();

            try
            {
                var resultDateTime = new DateTime();
                var allocationCollection = db.GetCollection<AllocationDetail>("AllocationDetail");
                var collection = db.GetCollection<Usage>("Usage");
                //usageDetail.UsageDate = new DateTime(usageDetail.UsageDate.Year, usageDetail.UsageDate.Month, usageDetail.UsageDate.Day, 0,0,0,0,DateTimeKind.Utc);

                var result = allocationCollection.AsQueryable().Where(x => x.Engineer == usageDetail.Engineer && x.Item == usageDetail.Item).OrderBy(x => x.AllocationDate).ToList();
                var resultWithDateFilter = allocationCollection.AsQueryable().Where(x => x.Engineer == usageDetail.Engineer &&  x.Item == usageDetail.Item && usageDetail.UsageDate.AddDays(1) > x.AllocationDate).OrderBy(x => x.AllocationDate).ToList();
                if(resultWithDateFilter.Count != 0)
                    resultDateTime = new DateTime(resultWithDateFilter.FirstOrDefault().AllocationDate.Year, resultWithDateFilter.FirstOrDefault().AllocationDate.Month, resultWithDateFilter.FirstOrDefault().AllocationDate.Day, 0, 0, 0, 0, DateTimeKind.Utc);

                int totalQuantityAllocated = resultWithDateFilter.AsQueryable().Sum(x => x.QuantityAllocated);

                if (result.Count() == 0)
                {
                    returnString.ErrorMessage = usageDetail.Item.ItemName + " is not allocated to " + usageDetail.Engineer.EngineerName;
                }
                else if (usageDetail.UsageDate < resultDateTime)
                {
                    returnString.ErrorMessage = usageDetail.Item.ItemName + " cannot be reported by " + usageDetail.Engineer.EngineerName + " for selected date. Please check allocation date";
                }
                else if (usageDetail.QuantityUsed > totalQuantityAllocated)
                {
                    returnString.ErrorMessage = usageDetail.Engineer.EngineerName + " can report only " + totalQuantityAllocated + " " + usageDetail.Item.ItemName + "/s for selected date.";
                }
                //else if(collection.Find(x => x.BillNumber == usageDetail.BillNumber).Any())
                //{
                //    returnString.ErrorMessage = "Bill number " + usageDetail.BillNumber + " already exists";
                //}
                
                else
                {
                    if (collection.CountDocuments(FilterDefinition<Usage>.Empty) == 0)
                    {
                        var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "UsageID");
                        db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                    }

                    usageDetail.UsageID = common.GetNextSequenceValue("UsageID", db);
                    collection.InsertOne(usageDetail);

                    returnString.SuccessMessage = "Data inserted successfully";
                    
                    while (usageDetail.QuantityUsed > 0)
                    {
                        var usedItem = allocationCollection.AsQueryable().Where(x => x.Engineer == usageDetail.Engineer && x.Item == usageDetail.Item).OrderBy(x => x.AllocationDate).FirstOrDefault();
                         
                        var quantity = usedItem.QuantityAllocated;

                        var setQuantity = quantity - usageDetail.QuantityUsed;
                        if (setQuantity > 0)
                        {
                            var allocationFilter = Builders<AllocationDetail>.Filter.Eq(x => x.AllocationID, usedItem.AllocationID);
                            var update = Builders<AllocationDetail>.Update.Set(x => x.QuantityAllocated, setQuantity);
                            var updatedDocument = allocationCollection.FindOneAndUpdate(allocationFilter, update);
                            usageDetail.QuantityUsed = 0;
                        }
                        else
                        {
                            if (setQuantity < 0)
                                usageDetail.QuantityUsed = usageDetail.QuantityUsed - quantity;
                            else if (setQuantity == 0)
                                usageDetail.QuantityUsed = 0;
                                
                            allocationCollection.DeleteOne<AllocationDetail>(x => x.AllocationID == usedItem.AllocationID);
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


        // POST: api/Usage
        [HttpPost]
        public string UpdateUsage(Usage usage)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Usage>("Usage");

                FilterDefinition<Usage> filter = Builders<Usage>.Filter.Eq(x => x.UsageID, usage.UsageID);
                UpdateDefinition<Usage> update = Builders<Usage>.Update
                    .Set(x => x.Item, usage.Item)
                    .Set(x => x.QuantityUsed, usage.QuantityUsed)
                    .Set(x => x.Engineer, usage.Engineer)
                    .Set(x => x.City, usage.City)
                    .Set(x => x.UsageDate, usage.UsageDate)
                    .Set(x => x.BillNumber, usage.BillNumber)
                    .Set(x => x.Warranty, usage.Warranty)
                    .Set(x => x.PartyName, usage.PartyName)
                    .Set(x => x.Amount, usage.Amount)
                    .Set(x => x.ServiceCharge, usage.ServiceCharge);

                collection.UpdateOne(filter, update);

                returnString.SuccessMessage = "Data updated successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // GET: api/Usage
        [HttpGet]
        public string DeleteUsage(int usageId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Usage>("Usage");

                FilterDefinition<Usage> filter = Builders<Usage>.Filter.Eq(x => x.UsageID, usageId);

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
