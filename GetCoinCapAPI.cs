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

            //const double bitcoinTrigger = 130000.00;

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



            // ------------   SQL Connection --------------- //
            
            string responseMessage = "";
            
            try 
            { 
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = "mbaes25-bicoin-serverdb.database.windows.net"; 
                    builder.UserID = "mydbadmin";            
                    builder.Password = "DBadmin!_";     
                    builder.InitialCatalog = "MBA-ES25-Bitcoin_DB";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");
                    
                    connection.Open();       

                    String sql = "SELECT * FROM myBitCoin";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                // responseMessage = "Connection OK \n"+ connection + "\n" + sql; 
                                responseMessage = string.Format("{0} {1}", reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }                    
                }
            }
            catch (SqlException e)
            {
                //Console.WriteLine(e.ToString());
                responseMessage = e.ToString();
            }

            // ------------   SQL Connection --------------- //
            

            // string varTriggerMail = "False"; 
            // if (varBitcoinBRL > bitcoinTrigger)
            //     {
            //         varTriggerMail = "True";
            //     }
            // else
            //     {
            //         varTriggerMail = "False";
            //     }
            
            DateTime timeStamp = DateTime.Now;

            // string responseMessage = string.IsNullOrEmpty(json)
            //     ? "This HTTP triggered function executed successfully!"
            //     : $"This HTTP triggered function executed successfully! \n CoinCap.io json respon is: \n USD = {varBitcoinUSD} \n BRL Rate = {varBRLRate} \n Bicoin BRL: {varBitcoinBRL} \n Send e-mail: {varTriggerMail} \n Timestamp: {timeStamp}";
                    
                
            return new OkObjectResult(responseMessage);
        }
    }
}
