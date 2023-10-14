// Youtube tutorial: https://www.youtube.com/watch?v=btmAfKz2ijY&t=538s
// API to get BitCoin value https://coincap.io
// https://api.coincap.io/v2/assets/bitcoin Get BitCoin 
// https://api.coincap.io/v2/rates/brazilian-real Get BRL Rate 
// https://zetcode.com/csharp/json/ Json handle
// https://www.youtube.com/watch?v=LZxm4A0qBa4 Connect to DB
// https://learn.microsoft.com/en-gb/samples/azure-samples/azure-sql-binding-func-dotnet-todo/todo-backend-dotnet-azure-sql-bindings-azure-functions/
// https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email?tabs=windows%2Cconnection-string&pivots=platform-azportal
// https://www.youtube.com/watch?v=t0in_d9Q2mU&t=8s

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Azure;
using Azure.Communication.Email; // SendE-mail Azure Communication Service
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
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

            // --- Start DateTimeStamp value --- //
            DateTime timeStamp = DateTime.Now;
            // --- End DateTimeStamp value --- //

            // --- Start Response Message iniciate --- //
            string responseMessage = "";
            // --- End Response Message iniciate --- //



            // --- Start Call API to get Bitcoin in USD --- //
            // string url = $"http://api.weatherstack.com/current?access_key={apiKey}&query={locationCityCountry}";
            string url = "https://api.coincap.io/v2/assets/bitcoin"; //API URL to get Asset Cost
            log.LogInformation($"url: {url}");

            var client = new HttpClient();
            var response = await client.GetAsync(url);

            // Handle the API response
            var json = await response.Content.ReadAsStringAsync();
            dynamic responseData = JsonConvert.DeserializeObject(json);
            dynamic varBitCoinPriceUsd = responseData.data.priceUsd;
            double varBitcoinUSD = Convert.ToDouble(varBitCoinPriceUsd);
            // --- End Call API to get Bitcoin in USD --- //

            // --- Start Call API to get USD/BRL rate  --- //
            // string url = $"http://api.weatherstack.com/current?access_key={apiKey}&query={locationCityCountry}";
            url = "https://api.coincap.io/v2/rates/brazilian-real"; //API URL to get Asset Cost
            log.LogInformation($"url: {url}");

            client = new HttpClient();
            response = await client.GetAsync(url);

            // Handle the API response
            json = await response.Content.ReadAsStringAsync();
            responseData = JsonConvert.DeserializeObject(json);
            dynamic varJsonBRLRate = responseData.data.rateUsd;
            double varBRLRate = Convert.ToDouble(varJsonBRLRate);
            // --- end Call API to get USD/BRL rate  --- //


            // --- Start Calc Bitcoin value in BRL --- //
            double varBitcoinBRL = varBitcoinUSD/varBRLRate;
            // --- Start Calc Bitcoin value in BRL --- //


            // --- Start Send E-mail --- //
            // Define Bitcoint Trigger to Send e-mail //
            const double bitcoinTrigger = 130000.00;
            
            // Validate trigger Flag //
            Boolean varTriggerMail = false;
            if (varBitcoinBRL < bitcoinTrigger)
                {
                    varTriggerMail = true;
                }
            else
                {
                    varTriggerMail = false;
                }

            // 
            
            if (varTriggerMail)
            {

            
                // This code retrieves your connection string from an environment variable.
                string connectionString = "endpoint=https://commservicesbitcoin.unitedstates.communication.azure.com/;accesskey=4opAEaMjX9pe3Qc0OSNNDscbbJcXtvqZe7BZ8mD/FTYhswz+CtqcuR/gZSka6eI2CSIoA2sG7QIwebv2HTVD3w==";
                var emailClient = new EmailClient(connectionString);

                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    Azure.WaitUntil.Completed,
                    "DoNotReply@85c4848c-c4d1-4f5e-b14a-7ef5f646ebcb.azurecomm.net",
                    "leosts18@gmail.com",
                    "Bitcoin - Preço Baixo ",
                    "<html>Olá,<br>Você está recebendo esse e-mail por que o valor da Bitcoint está abaixo de R$ 130mil.<br>Atenciosamente,<br>MBA-ES25-Grupo 2B</html>"
                    );
                try
                {
                    while (true)
                    {
                        await emailSendOperation.UpdateStatusAsync();
                        if (emailSendOperation.HasCompleted)
                        {
                            break;
                        }
                        await Task.Delay(100);
                    }

                    if (emailSendOperation.HasValue)
                    {
                        Console.WriteLine($"Email queued for delivery. Status = {emailSendOperation.Value.Status}");
                        responseMessage = $"Email queued for delivery. Status = {emailSendOperation.Value.Status}\n";
                    }
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"Email send failed with Code = {ex.ErrorCode} and Message = {ex.Message}");
                }

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            else
            {
                responseMessage = $"O valor em BRL do Bitcoin é {varBitcoinBRL}, maior que {bitcoinTrigger} reais. Não enviar e-mail.\n";
            }
            // --- End Send E-mail --- //


            // ------------ Start SQL Connection --------------- //            
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
                                responseMessage = responseMessage + "DB read OK! - " + timeStamp + "\n" + varSqlResult;
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
                responseMessage = responseMessage + "DB read KO | " + timeStamp + "\n" + e.ToString();
            }

            // ------------ End SQL Connection --------------- //
 

            //string responseMessage = string.IsNullOrEmpty(json)
            //    ? "This HTTP triggered function executed successfully!"
            //    : $"This HTTP triggered function executed successfully! \n CoinCap.io json respon is: \n USD = {varBitcoinUSD} \n BRL Rate = {varBRLRate} \n Bicoin BRL: {varBitcoinBRL} \n Send e-mail: {varTriggerMail} \n Timestamp: {timeStamp}";
                
            return new OkObjectResult(responseMessage);
        }
    }
}
