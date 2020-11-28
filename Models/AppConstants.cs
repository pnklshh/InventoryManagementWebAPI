using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagementWebAPI.Models
{
    public class AppConstants
    {
        public string Defective_ChallanNumber = "RTN";
        public string Default_ChallanNumber = "NA";
        public string FromAddress = "sa.inventorymanagement@gmail.com";
        public string Host = "smtp.gmail.com";
        public int Port = 587; // earlier worked with 25
        public string AccountPassword = "YmFyb2RhMTk5NQ=="; //previous: "aW52ZW50b3J5QDEyMw==";
    }
}