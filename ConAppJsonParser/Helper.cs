using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConAppJsonParser.Models;
using ConAppJsonParser.Models.Availability;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using Serilog;
using static System.Console;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ConAppJsonParser;

public class Helper
{
    private readonly ProcessingInput _config;
    public Helper(ProcessingInput config)
    {
        _config = config;
    }

    // Access Token
    //var accountsBaseUri = configSettings["Accounts.Host"];
    //var clientId = configSettings["ClientID"];
    //var clientSecret = configSettings["ClientSecret"];
    //var accountsClient = new AccountsClient(accountsBaseUri);

    //<!-- External API settings -->
    //<add key = "Accounts.Host" value="https://accountsint.iqmetrix.net" />
    //<add key = "ClientID" value="CovaTraceability" />
    //<add key = "ClientSecret" value="YbhxL6OLLSorUbTkWzoGxBIa" />
    
    
    public async Task<CatalogProductQuantitiesDetailed> GetCatalogProductQuantitiesDetailed(int companyId, int locationId, string catalogItem) 
    {
        Log.Information("Calling Availabilty with Company: {companyId}, Location: {locationId}, Catalog Item: {catalogItemId}", 
            companyId, locationId, catalogItem);
        AccessTokenResult token = await GetToken();
        //var companyId = 705656;
        //var locationId = 705664;
        RestClient restClient = new(_config.AvailabilityBaseUrl);
        bool catalogItemId = Guid.TryParse(catalogItem, out Guid guildCatalogItem);

        //var request = new RestRequest($"/v1/Companies({companyId})/Entities({locationId})/CatalogProductQuantitiesDetailed({guildResult})", Method.Get);
        var request = new RestRequest($"/v1/Companies({companyId})/entities({locationId})/CatalogProductQuantitiesDetailed({guildCatalogItem})");
        request.AddHeader("Accept", "application/json");

        restClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token.AccessToken, "Bearer");

        var results = restClient.GetAsync<CatalogProductQuantitiesDetailed>(request);
        return results.Result;
    }

    public Task<Inventory> GetBioTrackInventory()
    {
        var client = new RestClient();
        var request = new RestRequest(_config.BioTrackApi, Method.Post);
        request.AddHeader("Accept", "application/json");

        //GreenLeaf
        //var body = new BioTrackRequest
        //{
        //    Action = "sync_inventory",
        //    LicenseNumber = "333333035",
        //    UserName = "biotracksucks@schwazze.com",
        //    Password = "qM56areWXY",
        //    Training = "0",
        //    Active = "0",
        //};

        //CowboyVerde
        var body = new BioTrackRequest
        {
            Action = "sync_inventory",
            LicenseNumber = "422000058",
            UserName = "cowboyverde@gmail.com",
            Password = "Dallas3333",
            Training = "1",
            Active = "0",
        };

        request.AddBody(body);
        Log.Information("Requesting BioTrack for ALL items.");
        var watch = Stopwatch.StartNew();
        var response = client.ExecutePost<Inventory>(request);
        watch.Stop();
        var elapsedTime = watch.Elapsed.TotalSeconds;
        Log.Information("Fetched no. of items: {itemsCount} in: {elapsedTime}", response.Data.InventoryItems.Count, elapsedTime);
        return Task.FromResult(response.Data);

        //Inventory responseData = JsonSerializer.Deserialize<Inventory>(response.Content);
        //WriteLine($"Company: {body.LicenseNumber} has ALL Inventory count: {responseData.InventoryItems.Count}");
        //WriteLine($"Payload response size: {Encoding.UTF8.GetBytes(response.Content).Length} bytes.\nFetch time in ms:\t{elapsedTime} sec.");
        //return Task
    }

    public static IEnumerable<FailedInvoiceInput> GetFailedInvoices(string fileToParse)
    {
        ArgumentNullException.ThrowIfNull(fileToParse, nameof(fileToParse));
        Log.Information("Input file name and location: {fileToParse}", fileToParse);
        var data = File.ReadAllText(fileToParse);
        var failedItems = JsonSerializer.Deserialize<IEnumerable<FailedInvoiceInput>>(data).ToList();
        Log.Information("Number of records to Process: {failedItemsCount}", failedItems.Count);
        return failedItems;
    }

    public Task AdjustPackageQuantity(string packageId, string qty)
    {
        packageId = "7842825485478757";
        qty = "5000";

        var client = new RestClient();
        var request = new RestRequest(_config.BioTrackApi, Method.Post);
        request.AddHeader("Accept", "application/json");

        var body = new BioTrackRequest
        {
            Action = "inventory_adjust",
            LicenseNumber = _config.LicenseNumber,
            UserName = _config.UserName,
            Password = _config.Password,
            Training = "1"
        };

        body.Payload.Add(new AdjustmentData { BarcodeId = packageId, Quantity = qty });

        request.AddBody(body);
        Log.Information("Request to Adjust Item: {item} with Qty: {qty}", packageId, qty);

        var response = client.ExecutePost<Inventory>(request);
        return Task.FromResult(response.Data);
    }


    public static void ProcessFailedAdjustments(string data, Inventory responseData)
    {
        var failedItems = JsonSerializer.Deserialize<IEnumerable<FailedAdjustmentItem>>(data).ToList();

        List<KeyValuePair<string, FailedAdjustmentItem>> IdList = new();
        foreach (var item in failedItems)
        {
            var myItem = Regex.Replace(item.ProcessorResponse, @"[^\d]", "");
            Match match = Regex.Match(myItem, @"^\d{16}$");
            if (match.Success)
            {
                IdList.Add(new KeyValuePair<string, FailedAdjustmentItem>(myItem, item));
            }
        }

        WriteLine($"List of Inventory Items (Count: {IdList.Count}) extracted from FailedAdjustment JSON file:\n");
        var items = IdList.Select((value, index) => new { value, index }).ToList();
        items.ForEach(x => WriteLine($"{x.index}.\t{x.value.Key}"));
        WriteLine();

        foreach (var item in IdList)
        {
            var btItem = responseData.InventoryItems.Where(x => x.Id == item.Key).First();

            var qty = int.Parse(btItem.Remaining_Quantity);
            if (btItem is not null)
            {
                var consColor = qty > 0
                    ? ForegroundColor = ConsoleColor.Green
                    : ForegroundColor = ConsoleColor.DarkRed;
                ForegroundColor = consColor;

                WriteLine($"Reason: {item.Value.ProcessorResponse}" +
                    $"\nBioTract Data: {btItem.Id} has Remaining Qty: {btItem.Remaining_Quantity}" +
                    $"\nAdjustment's Azure ID: {failedItems.Where(x => x.Id == item.Value.Id).First().Id}");
                WriteLine();
            }
            else
            {
                ForegroundColor = ConsoleColor.Blue;
                WriteLine($"Item: {item.Key} was not found in the processing file.");
            }
        }
        ForegroundColor = ConsoleColor.White;
    }

    public static void SaveToCSV(List<AdjustmentResult> users)
    {
        ArgumentNullException.ThrowIfNull(users, nameof(users));
        if (users.Count == 0) return;
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        };

        //using (var mem = new MemoryStream())
        using var writer = new StreamWriter("c:\\temp\\JsonProcessing\\AdjustmentResults.csv");
        //using (var writer = new StreamWriter(mem))
        using var csvWriter = new CsvWriter(writer, csvConfig);
        csvWriter.WriteHeader<AdjustmentResult>();
        csvWriter.NextRecord();
        csvWriter.WriteRecords(users);

        writer.Flush();
        //var result = Encoding.UTF8.GetString(mem.ToArray());
        //Console.WriteLine(result);
        Console.WriteLine("Done Saving csv check the folder.");
    }

    public async Task<RestResponse> GetCovaInventoryItemBulk(string companyId)
    {
        ArgumentNullException.ThrowIfNull(companyId, "CompanyId");
        RestClient restClient = new("https://catalogsint.iqmetrix.net");
        AccessTokenResult token = await GetToken();
        var request = new RestRequest($"/v1/companies({companyId})/catalog/items", Method.Get);
        request.AddHeader("Accept", "application/json");

        restClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token.AccessToken, "Bearer");


        var results = restClient.Get(request);

        return results;
    }

    private Task<AccessTokenResult> GetToken()
    {
        var restClient = new RestClient(_config.AccountsBaseUrl);
        var request = new RestRequest("/v1/oauth2/token", Method.Post);

        request.AddHeader("content-type", "application/x-www-form-urlencoded");
        request.AddParameter("grant_type", "client_credentials");
        request.AddParameter("client_id", "CovaTraceability");
        request.AddParameter("client_secret", "YbhxL6OLLSorUbTkWzoGxBIa");

        RestResponse response = restClient.ExecutePost<AccessTokenResult>(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to request access token");

        var responseData = JsonSerializer.Deserialize<AccessTokenResult>(response.Content);
        //var expiresIn = double.Parse((string)data.expires_in);
        //var tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn).AddMinutes(-30);
        return Task.FromResult(responseData);
    }

}

