using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace SQL_Upload
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string query = run_cmd();
                query = query.Substring(1, query.Length - 4);
                List<string> temp_array = query.Split('"').ToList();
                List<string> query_list = new List<string>();

                foreach (var item in temp_array)
                {
                    if (item.ToString().StartsWith("INSERT"))
                    {
                        query_list.Add(item.ToString());
                    }
                }

                Console.WriteLine("==================================DONE SCRAPING=================================");

                foreach (var item in query_list)
                {
                    Thread.Sleep(2000);
                    ConnectionString(item.ToString());
                }

                // Run the examples asynchronously, wait for the results before proceeding
                ProcessAsync().GetAwaiter().GetResult();

                Console.WriteLine("===============================FINISHED UPLOADING TO AZURE BLOB====================================");
                Thread.Sleep(10800000);
            }
        }

        private static string run_cmd()
        {
            string filename = @"C:/Users/ersenergy/Desktop/Khee_Intern/PythonWebScraper/PythonWebScraper/PythonWebScraper_v2.py";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:/Users/ersenergy/AppData/Local/Programs/Python/Python37-32/python.exe", filename)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        // SQL START
        public static SqlConnectionStringBuilder ConnectionString(string line)
        {
            SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder();

            sql.DataSource = "sqlsever-ers.database.windows.net";   // Server name from azure
            sql.UserID = "ers"; // ID to access DB
            sql.Password = "testing123#";   //password to access DB
            sql.InitialCatalog = "inverterDB";  //Database name
            StringToSql(line, sql);

            return sql;
        }

        public static void StringToSql(string line, SqlConnectionStringBuilder sql)
        {


            //formatting strings to meet sql query syntax requirement
            string sendQuery = line ; // STRING HERE
                

            // Connecting to sql and execute query formed above
            using (SqlConnection sqlconn = new SqlConnection(sql.ConnectionString))
            {
                String sqlquery = sendQuery.ToString();
                SqlCommand sqlCommand = new SqlCommand(sqlquery, sqlconn);
                try
                {
                    sqlconn.Open();
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine(sqlquery);
                    Console.WriteLine("==================================Uploaded to SQL====================================");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }
        }

        // SQL END

        // AZURE BLOB START

        private static async Task ProcessAsync()
        {
            var txtPath = "C:/Users/ersenergy/Desktop/Khee_Intern/Scraped_Graph";
            string[] files = Directory.GetFiles(txtPath);


            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=ers123storage;AccountKey=II7tUyujVf0et5CKQvytRzFcN7EJUY9NK4Sh42paFL6ogi8A9RElKII1+cvFLDqu6/2Vm3l1php4hjDU71ue5w==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob
                // storage here.
                // ADD OTHER OPERATIONS 
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("graphcontainer");

                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };


                await cloudBlobContainer.SetPermissionsAsync(permissions);



                foreach (string file_path in files)
                {
                    string sourceFile = file_path;
                    string localFileName = file_path.Substring(53);
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);
                }

                foreach (string file_path in files)
                {
                    File.Delete(file_path);
                }

            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'CONNECT_STR' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadLine();
            }
        }

        // AZURE BLOB END

    }
}
