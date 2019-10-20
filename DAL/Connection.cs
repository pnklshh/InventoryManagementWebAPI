using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;



namespace InventoryManagementWebAPI.DAL
{
    public class Connection
    {
        public MySqlConnection connection;
        private string server = "localhost";
        private string port = "3306";
        private string database = "inventory";
        private string uid = "root";
        private string password = null;
        private string sslmode = "none";

        public Connection()
        {
            string connectionString;
            connectionString = "server=" + server + ";" + "port=" + port + ";" + "database=" +
            database + ";" + "userid=" + uid + ";" + "password=" + password + ";" + "sslmode=" + sslmode + ";";

            connection = new MySqlConnection(connectionString);
        }

        
    }
}