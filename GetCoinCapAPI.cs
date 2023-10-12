// Youtube tutorial: https://www.youtube.com/watch?v=btmAfKz2ijY&t=538s
// API to get BitCoin value https://coincap.io
// https://api.coincap.io/v2/assets/bitcoin Get BitCoin 
// https://api.coincap.io/v2/rates/brazilian-real Get BRL Rate 
// https://zetcode.com/csharp/json/ Json handle
// https://www.youtube.com/watch?v=LZxm4A0qBa4 Connect to DB
// https://learn.microsoft.com/en-gb/samples/azure-samples/azure-sql-binding-func-dotnet-todo/todo-backend-dotnet-azure-sql-bindings-azure-functions/


using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Data.Common; // need for http client to call HTTP API
//using JsonDocument; // Json parse
using System.Data.SqlClient;
using Microsoft.Data.SqlClient; 

namespace GetCoincapAPI.Function
{

    public static class GetCoinCapAPI
    {
        [FunctionName("GetCoinCapAPI")]



        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] 
            HttpRequest req, 
            ILogger log
            )
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            const double bitcoinTrigger = 130000.00;

            // API call URL preparation
            // string url = $"http://api.weatherstack.com/current?access_key={apiKey}&query={locationCityCountry}";
            string url = "https://api.coincap.io/v2/assets/bitcoin"; //API URL to get Asset Cost
            log.LogInformation($"url: {url}");

            // Initiate http call with weatherstack api
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            // Handle the API response
            var json = await response.Content.ReadAsStringAsync();
            dynamic responseData = JsonConvert.DeserializeObject(json);
            dynamic varBitCoinPriceUsd = responseData.data.priceUsd;
            double varBitcoinUSD = Convert.ToDouble(varBitCoinPriceUsd);


            // API call URL preparation
            // string url = $"http://api.weatherstack.com/current?access_key={apiKey}&query={locationCityCountry}";
            url = "https://api.coincap.io/v2/rates/brazilian-real"; //API URL to get Asset Cost
            log.LogInformation($"url: {url}");

            // Initiate http call with weatherstack api
            client = new HttpClient();
            response = await client.GetAsync(url);

            // Handle the API response
            json = await response.Content.ReadAsStringAsync();
            responseData = JsonConvert.DeserializeObject(json);
            dynamic varJsonBRLRate = responseData.data.rateUsd;
            double varBRLRate = Convert.ToDouble(varJsonBRLRate);

            double varBitcoinBRL = varBitcoinUSD/varBRLRate;

            //string varTriggerMail = "False"; 
            Boolean varTriggerMail = false;
            if (varBitcoinBRL < bitcoinTrigger)
                {
                    varTriggerMail = true;
                }
            else
                {
                    varTriggerMail = false;
                }
            
            DateTime timeStamp = DateTime.Now;




            // ------------   SQL Connection --------------- //
            
            string responseMessage = "OK";
            
            try 
            { 
                // Making the DB Connection String //
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = "mbaes25-bicoin-serverdb.database.windows.net"; 
                    builder.UserID = "mydbadmin";            
                    builder.Password = "DBadmin!_";     
                    builder.InitialCatalog = "MBA-ES25-Bitcoin_DB";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    // Executing the connection
                    connection.Open();       


                    // Query the 1st line Desc BEFORE add another line
                    String sql = "SELECT TOP 1 * FROM myBitCoin ORDER BY id Desc";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string varSqlResult = String.Format("{0} {1} {2}", reader["id"], reader["usdValue"], reader["timeStamp"]);
                                Console.WriteLine(varSqlResult);
                                responseMessage = "DB read OK! - " + timeStamp + "\n" + varSqlResult;
                            }
                        }
                    }

                    // Query to INSERT a new line with API result using the same connection
                    sql = "INSERT INTO myBitCoin (usdValue, brlRate, brlValue, sendEmail, timeStamp) VALUES (@usdValue, @brlRate, @brlValue, @sendEmail, @timeStamp)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@usdValue",varBitcoinUSD);
                        command.Parameters.AddWithValue("@brlRate",varBRLRate);
                        command.Parameters.AddWithValue("@brlValue",varBitcoinBRL);
                        command.Parameters.AddWithValue("@sendEmail",varTriggerMail);
                        command.Parameters.AddWithValue("@timeStamp",timeStamp);
                        command.ExecuteNonQuery();
                    }

                    // Query the 1st line Desc AFTER added line
                    sql = "SELECT TOP 1 * FROM myBitCoin ORDER BY id Desc";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                         using (SqlDataReader reader = command.ExecuteReader())
                         {
                             while (reader.Read())
                             {
                                 string varSqlResult = String.Format("{0} {1} {2}", reader["id"], reader["usdValue"], reader["timeStamp"]);
                                 Console.WriteLine(varSqlResult);
                                 responseMessage = responseMessage + "\n" + varSqlResult;
                             }
                         }
                    }
                // Close DB connection
                connection.Close();
                }
            }
            catch (SqlException e)
            {
                // Handle SQL error by display error message
                responseMessage = "DB read KO | " + timeStamp + "\n" + e.ToString();
            }

            // ------------   SQL Connection --------------- //
      
            //string responseMessage = string.IsNullOrEmpty(json)
            //    ? "This HTTP triggered function executed successfully!"
            //    : $"This HTTP triggered function executed successfully! \n CoinCap.io json respon is: \n USD = {varBitcoinUSD} \n BRL Rate = {varBRLRate} \n Bicoin BRL: {varBitcoinBRL} \n Send e-mail: {varTriggerMail} \n Timestamp: {timeStamp}";
                
            return new OkObjectResult(responseMessage);
        }
    }
}
