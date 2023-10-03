// Youtube tutorial: https://www.youtube.com/watch?v=btmAfKz2ijY&t=538s
// API to get BitCoin value https://coincap.io
// https://api.coincap.io/v2/assets/bitcoin :Get BitCoin 
// https://api.coincap.io/v2/rates/brazilian-real :Get BRL Rate 
// https://zetcode.com/csharp/json/ :Json handle

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http; // need for http client to call HTTP API
using JsonDocument; // Json parse



namespace GetCoincapAPI.Function
{
    public static class GetCoinCapAPI
    {
        [FunctionName("GetCoinCapAPI")]
        
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            // API call URL preparation
            // string url = $"http://api.weatherstack.com/current?access_key={apiKey}&query={locationCityCountry}";
            string url = "https://api.coincap.io/v2/assets/bitcoin";
            log.LogInformation($"url: {url}");

            // Initiate http call with weatherstack api
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            // Handle the http response
            var json = await response.Content.ReadAsStringAsync();
            dynamic responseData = JsonConvert.DeserializeObject(json);

            // To Test
            //doc = JsonDocument.Parse(json);
            //JsonElement root = doc.RootElement;

            string responseMessage = string.IsNullOrEmpty(json)
            ? "<h1>This HTTP triggered function executed successfully!</h1>"
                : $"<h1>This HTTP triggered function executed successfully!</h1> \n CoinCap.io json respon is \n {json}";

            return new OkObjectResult(responseMessage);
        }
    }
}
