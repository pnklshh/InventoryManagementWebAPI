using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InventoryManagementWebAPI.DAL;
using InventoryManagementWebAPI.Models;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Http;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;
using System.Configuration;

namespace InventoryManagementWebAPI.Controllers
{
    public class CommonController : ApiController
    {
        MongoConnection connection = new MongoConnection();
        IMongoDatabase db;
        OutputFormat returnString = new OutputFormat();

        // GET: Common
        public int GetNextSequenceValue(string sequenceName, IMongoDatabase database)
        {
            var collection = database.GetCollection<Sequence>("Sequence");
            var filter = Builders<Sequence>.Filter.Eq(a => a.Name, sequenceName);
            var update = Builders<Sequence>.Update.Inc(a => a.Value, 1);
            var sequence = collection.FindOneAndUpdate(filter, update);
            
            if (sequence == null)
            {
                Sequence seq = new Sequence();
                seq.Name = sequenceName;
                seq.Value = 1;
                collection.InsertOne(seq);
                return seq.Value;
            }
            return sequence.Value + 1;
        }

        

        // GET: api/Common
        [HttpGet]
        public string ValidateLogin(string emailId, string password)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            List<Engineer> engineerList = new List<Engineer>();

            try
            {
                var collection = db.GetCollection<Engineer>("Engineer");

                if(collection.Find(x => x.EmailID == emailId && x.Password == password).CountDocuments() == 1)
                {
                    var engineer = collection.Find(x => x.EmailID == emailId && x.Password == password).FirstOrDefault();
                    returnString.SuccessMessage = "Data fetched successfully";
                    returnString.AppData = JsonConvert.SerializeObject(engineer);
                }
                else if (collection.Find(x => x.EmailID == emailId).CountDocuments() == 0)
                {
                    returnString.ErrorMessage = "No user with this email id";
                }
                else if(collection.Find(x => x.EmailID == emailId && x.Password != password).CountDocuments() == 1)
                {
                    returnString.ErrorMessage = "Password invalid";
                }
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }

        [HttpGet]
        public string SendEmail(string emailId)
        {
            db = connection.database;
            returnString.SuccessMessage = "";
            returnString.ErrorMessage = "";
            returnString.AppData = "";

            try
            {
                AppConstants constants = new AppConstants();
                var collection = db.GetCollection<Engineer>("Engineer");
                var userExist = collection.AsQueryable().Where(x => x.EmailID.ToLower() == emailId.ToLower()).ToList();

                if(userExist.Count == 0)
                {
                    throw new Exception("No user with this email id");
                }

                string userName = emailId.Substring(0, emailId.IndexOf('@'));
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["FromAddress"]);
                    mail.To.Add(emailId);
                    mail.Subject = "Reset Password";
                    mail.Body = "Hi " + userName + ",<br/><br/>Click on the link given below to reset the password for your Inventory Management account.<br/>"+ ConfigurationManager.AppSettings["UIHostUrl"] + "#/auth/resetpassword/" + Convert.ToBase64String(Encoding.UTF8.GetBytes(emailId)) + "<br/><br/> Thanks,<br/>SuperAdmin";
                    mail.IsBodyHtml = Convert.ToBoolean(ConfigurationManager.AppSettings["IsBodyHTML"]);
                    //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = ConfigurationManager.AppSettings["Host"];
                        smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                        smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSL"]);
                        //string password = Encoding.UTF8.GetString(Convert.FromBase64String(constants.AccountPassword));
                        //smtp.Credentials = new NetworkCredential(constants.FromAddress, password);    // Credentials required if running locally
                        smtp.Send(mail);
                        returnString.SuccessMessage = "Mail sent successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }


        //[HttpPost]
        //public string SendEmail([FromBody]JObject body)
        //{
        //    db = connection.database;
        //    returnString.SuccessMessage = "";
        //    returnString.ErrorMessage = "";
        //    returnString.AppData = "";

        //    try
        //    {
        //        //dynamic body = obj;
        //        string mailSubject = body["Subject"] != null ? body["Subject"].ToString() : "";
        //        string mailBody = body["Body"] != null ? body["Body"].ToString() : "";
        //        string mailTo = body["To"] != null ? body["To"].ToString() : "";
        //        AppConstants constants = new AppConstants();
        //        string userName = mailTo.Substring(0, mailTo.IndexOf('@'));

        //        using (MailMessage mail = new MailMessage())
        //        {
        //            mail.From = new MailAddress(ConfigurationManager.AppSettings["FromAddress"]);
        //            mail.To.Add(mailTo);
        //            mail.Subject = mailSubject;
        //            mail.Body = mailBody; 
        //            //"Hi " + userName + ",<br/><br/>Click on the link given below to reset the password for your Inventory Management account.<br/>http://localhost:4200/#/auth/resetpassword/" + Convert.ToBase64String(Encoding.UTF8.GetBytes(emailId)) + "<br/><br/> Thanks,<br/>SuperAdmin";
        //            mail.IsBodyHtml = Convert.ToBoolean(ConfigurationManager.AppSettings["IsBodyHTML"]);
        //            //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
        //            using (SmtpClient smtp = new SmtpClient())
        //            {
        //                smtp.Host = ConfigurationManager.AppSettings["Host"];
        //                smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        //                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSL"]);
        //                //string password = Encoding.UTF8.GetString(Convert.FromBase64String(constants.AccountPassword));
        //                //smtp.Credentials = new NetworkCredential(constants.FromAddress, password);    // Credentials required if running locally
        //                smtp.Send(mail);
        //                returnString.SuccessMessage = "Mail sent successfully";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        returnString.ErrorMessage = ex.Message;
        //    }

        //    return JsonConvert.SerializeObject(returnString);
        //}


        // POST: api/Common
        [HttpGet]
        public string ResetPassword(string emailId, string newPassword)
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

                FilterDefinition<Engineer> filter = Builders<Engineer>.Filter.Eq(x => x.EmailID, emailId);
                UpdateDefinition<Engineer> update = Builders<Engineer>.Update
                    .Set(x => x.Password, newPassword);
                    
                collection.UpdateOne(filter, update);

                FilterDefinition<AllocationDetail> filter1 = Builders<AllocationDetail>.Filter.Eq(x => x.Engineer.EmailID, emailId);
                UpdateDefinition<AllocationDetail> update1 = Builders<AllocationDetail>.Update
                    .Set(x => x.Engineer.Password, newPassword);
                allocationCollection.UpdateMany(filter1, update1);

                FilterDefinition<Usage> filter2 = Builders<Usage>.Filter.Eq(x => x.Engineer.EmailID, emailId);
                UpdateDefinition<Usage> update2 = Builders<Usage>.Update
                    .Set(x => x.Engineer.Password, newPassword);
                usageCollection.UpdateMany(filter2, update2);

                returnString.SuccessMessage = "Password reset successfully";

                string data = ValidateLogin(emailId, newPassword);
                returnString.AppData = JsonConvert.DeserializeObject<OutputFormat>(data).AppData;
            }
            catch (Exception ex)
            {
                returnString.ErrorMessage = ex.Message;
            }

            return JsonConvert.SerializeObject(returnString);
        }
    }
}