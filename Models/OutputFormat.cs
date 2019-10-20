using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagementWebAPI.Models
{
    public class OutputFormat
    {
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string AppData { get; set; }
    }
}