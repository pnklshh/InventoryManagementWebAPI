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
    public class GroupController : ApiController
    {
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;
        OutputFormat returnString = new OutputFormat();

        // GET: api/Group
        [HttpGet]
        public string GetAllGroups()
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<Group> groupList = new List<Group>();

            try
            {
                var collection = db.GetCollection<Group>("Group");
                groupList = collection.Find(FilterDefinition<Group>.Empty).ToList().OrderBy(x => x.GroupName).ToList();

                returnString.SuccessMessage = "Data fetched successfully";
                returnString.AppData = JsonConvert.SerializeObject(groupList);
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);

        }

        // POST: api/Group
        [HttpPost]
        public string AddGroup(Group group)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            CommonController common = new CommonController();

            try
            {
                var collection = db.GetCollection<Group>("Group");

                var nameFilter = collection.AsQueryable().Where(x => x.GroupName.ToLower() == group.GroupName.ToLower()).ToList();
                if (nameFilter.Count > 0)
                {
                    throw new Exception("Group name already exists");
                }

                if (collection.CountDocuments(FilterDefinition<Group>.Empty) == 0)
                {
                    var filter = Builders<Sequence>.Filter.Eq(a => a.Name, "GroupID");
                    var coll = db.GetCollection<Sequence>("Sequence").FindOneAndDelete(filter);
                }

                group.GroupID = common.GetNextSequenceValue("GroupID", db);
                collection.InsertOne(group);
                returnString.SuccessMessage = "Data inserted successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Group
        [HttpPost]
        public string UpdateGroup(Group group)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Group>("Group");

                var nameFilter = collection.AsQueryable().Where(x => x.GroupName.ToLower() == group.GroupName.ToLower()).ToList();
                if (nameFilter.Count > 0)
                {
                    throw new Exception("Group name already exists");
                }

                FilterDefinition<Group> filter = Builders<Group>.Filter.Eq(x => x.GroupID, group.GroupID);
                UpdateDefinition<Group> update = Builders<Group>.Update
                    .Set(x => x.GroupName, group.GroupName);

                collection.UpdateOne(filter, update);

                returnString.SuccessMessage = "Data updated successfully";

            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        // POST: api/Group
        [HttpGet]
        public string DeleteGroup(int groupId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                var collection = db.GetCollection<Group>("Group");

                FilterDefinition<Group> filter = Builders<Group>.Filter.Eq(x => x.GroupID, groupId);

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
