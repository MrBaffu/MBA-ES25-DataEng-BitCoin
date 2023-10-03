// Youtube tutorial: https://www.youtube.com/watch?v=btmAfKz2ijY&t=538s
//API to get BitCoin value https://coincap.io
// https://api.coincap.io/v2/assets/bitcoin


#r "Newtonsoft.Json"
#r "System.Net.Http" // need for http client to call HTTP API

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net.Http; // need for http client to call HTTP API

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    // const string apiKey = "" // if needed
    // string DefaultAssetID = "bitcoin" // For this project it`s static.

    log.LogInformation("C# HTTP trigger function processed a request.");

    // string name = req.Query["AssetID"]; // Add the AssetID to the request

    // log.LogInformation($"req AssetID: {AssetID}"); // Add the AssetID requested to log
    // string assetID = string.IsNullOrEmpty (AssetID) ? DefaultAssetID : AssetID;

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


    string responseMessage = string.IsNullOrEmpty(json)
        ? "This HTTP triggered function executed successfully! "
            : $"This HTTP triggered function executed sucessfully! \n CoinCap.io json respon is \n {json}";
            return new OkObjectResult(responseMessage);
}