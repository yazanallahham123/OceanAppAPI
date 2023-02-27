using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Core
{
    public class DataConnection
    {

        public static SqlConnection Con_instance = null;
        //TODO: Handle change for connection string from ConnectionString
        public static string DatabaseName = string.Empty;

        // Lock synchronization object
        private static object syncLock = new object();

        public static SqlConnection GetDataConnection()
        {

           var config = new ConfigurationBuilder()
                .AddJsonFile(Directory.GetCurrentDirectory() + "/Properties/launchSettings.json", optional: true,
                reloadOnChange: true)
                .AddJsonFile(Directory.GetCurrentDirectory() + "/appsettings.json", optional: true,
                reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            if (Con_instance == null)
            {
                lock (syncLock)
                {
                    if (Con_instance == null)
                    {
                        string connectionString = config.GetSection("Connection")["ConnectionString"].ToString();
                        return new SqlConnection(connectionString);

                        #region TestingRegion
                        //Get database name
                        //IDbConnection connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["Connection"].ConnectionString);
                        //var dbName = connection.Database;
                        //if (dbName.ToLower() != "null")
                        //{
                        //   return new SqlConnection(WebConfigurationManager.ConnectionStrings["Connection"].ConnectionString);
                        //} 
                        #endregion

                        //New Added
                        //DatabaseName = "PointsSystemTest";
                        //int index = WebConfigurationManager.ConnectionStrings["Connection"].ConnectionString.IndexOf("Catalog=") + 1;
                        //string piece = WebConfigurationManager.ConnectionStrings["Connection"].ConnectionString.Substring(index);
                        //int indexOfComma = piece.IndexOf(";") + 1;
                        //string CatalogName = piece.Substring(0, indexOfComma);

                        //if (CatalogName == null)
                        //   CatalogName = DatabaseName;

                        //Save to config file 
                        //var configuration = WebConfigurationManager.OpenWebConfiguration("~");
                        //var section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
                        //section.ConnectionStrings["Connection"].ConnectionString = "Data Source=...";
                        //configuration.Save();



                        //return new SqlConnection(Con_String_p1 + CatalogName + Con_String_p2);

                        //return new SqlConnection(WebConfigurationManager.ConnectionStrings["Connection"].ConnectionString);

                    }

                }
            }

            return Con_instance;
        }
    }
}
