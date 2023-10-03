using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http; // need for http client to call HTTP API

namespace MyBitCoinAPITimeTrigger
{
    public class MyBitCoinAPI_TimeTrigger
    {
        [FunctionName("MyBitCoinAPI_TimeTrigger")]
        public void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string url = "https://mybitcoinapi.azurewebsites.net/api/GetCoinCapAPI?"; //API URL to get Asset Cost
            log.LogInformation($"url: {url}");

            var client = new HttpClient();
            var response = client.GetAsync(url);

        }
    }
}
