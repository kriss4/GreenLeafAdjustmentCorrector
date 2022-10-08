using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using ConAppJsonParser.Models;
using ConAppJsonParser.Models.Availability;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using Serilog;

namespace ConAppJsonParser;

public class Helper
{
    private readonly ProcessingInput _config;
    public Helper(ProcessingInput config)
    {
        _config = config;
    }
    
    
    public async Task<CatalogProductQuantitiesDetailed> GetCatalogProductQuantitiesDetailed(int companyId, int locationId, string catalogItem) 
    {
        Log.Information("Calling Availabilty with Company: {companyId}, Location: {locationId}, Catalog Item: {catalogItemId}", 
            companyId, locationId, catalogItem);

        //Below can be enabled in INT to dynamicaly generate token. Otherwise set token in appsettings.json
        //AccessTokenResult token = await GetToken(); 

        
        bool catalogItemId = Guid.TryParse(catalogItem, out Guid guildCatalogItem);
        if (!catalogItemId)
            Log.Error("Conversion of Catalog Item: {catalogItem} failed.", catalogItem);

        var request = new RestRequest($"/v1/Companies({companyId})/entities({locationId})/CatalogProductQuantitiesDetailed({guildCatalogItem})", Method.Get);
        request.AddHeader("Accept", "application/json");

        RestClient restClient = new(_config.AvailabilityBaseUrl)
        {
            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_config.AccessToken, "Bearer")
        };

        try
        {
            var results = await restClient.GetAsync<CatalogProductQuantitiesDetailed>(request).ConfigureAwait(false);
            return results;
        }
        catch (Exception ex)
        {
            Log.Error("Failed getting Catalog Item from Availability Service. Please check if Client Token is Valid. Reason: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Inventory> GetBioTrackInventory()
    {
        var request = new RestRequest(_config.BioTrackApi, Method.Post);
        request.AddHeader("Accept", "application/json");

        var body = new BioTrackRequest
        {
            Action = "sync_inventory",
            LicenseNumber = _config.LicenseNumber,
            UserName = _config.UserName,
            Password = _config.Password,
            Training = "0",
            Active = "0",
        };

        request.AddBody(body);
        Log.Information("Requesting BioTrack for ALL items.");
        var client = new RestClient();
        var watch = Stopwatch.StartNew();
        var response = await client.ExecutePostAsync<Inventory>(request).ConfigureAwait(false);
        watch.Stop();
        var elapsedTime = watch.Elapsed.TotalSeconds;
        if (response.IsSuccessful)
        {
            Log.Information("Fetched no. of items: {itemsCount} in: {elapsedTime}", response.Data.InventoryItems.Count, elapsedTime);
            return response.Data;
        }
        else {
            Log.Error("Failed fetching inventory from BioTrackTHC: {@response}", response);
            throw new ApplicationException("Failed to Fetch Inventory from BioTrack.");
        }
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

    public async Task<AdjustmentResult> AdjustPackageQuantity(FailedInvoiceInput inputRecord,  CatalogProductQuantitiesDetailed covaRecord)
    { 
        var client = new RestClient();
        var request = new RestRequest(_config.BioTrackApi, Method.Post);
        request.AddHeader("Accept", "application/json");

        var body = new BioTrackRequest
        {
            Action = "inventory_adjust",
            LicenseNumber = _config.LicenseNumber,
            UserName = _config.UserName,
            Password = _config.Password,
            Training = "0"
        };

        var qtyToAdjust = CalculateQty(covaRecord, inputRecord);
        body.Payload.Add(new AdjustmentData { BarcodeId = inputRecord.PackageId, Quantity = qtyToAdjust });

        request.AddBody(body);
        Log.Information("Item to Adjust: {item}. Qty Cova: {qtyCova} Qty to set: {qtyToAdjust}", inputRecord.PackageId, Convert.ToInt32(covaRecord.QuantityInStock), qtyToAdjust);

        var response = await client.ExecutePostAsync<AdjustmentResult>(request).ConfigureAwait(false);
        Log.Information("Adjustment request status: {responseStatus} and Transaciton ID: {transactionId}", 
            response.StatusCode, response.Data.Transactionid);
        if (response.IsSuccessful)
        {
            Log.Information("Successfully adjusted Package: {packageId} in Biotrack.", inputRecord.PackageId);
        }
        else {
            Log.Error("Failed Adjust Packed Id: {package} with return status: {@response}", inputRecord.PackageId, response);
        }

        return response.Data;
    }

    public static void SaveToCSV(List<AdjustmentReport> reportRecord)
    {
        ArgumentNullException.ThrowIfNull(reportRecord, nameof(reportRecord));
        if (reportRecord.Count == 0) return;
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        };

        using var writer = new StreamWriter("c:\\temp\\JsonProcessing\\AdjustmentResults.csv");
        using var csvWriter = new CsvWriter(writer, csvConfig);
        csvWriter.WriteHeader<AdjustmentReport>();
        csvWriter.NextRecord();
        csvWriter.WriteRecords(reportRecord);

        writer.Flush();
        Log.Information("Done Saving csv check the folder.");
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

    public static AdjustmentReport GenerateReportRecord(FailedInvoiceInput failedInvoice, 
        CatalogProductQuantitiesDetailed catalogItem, InventoryItem  btItem)
    {
        AdjustmentReport record = new()
        {
            InvoiceId = failedInvoice.InvoiceId,
            CatalogItemId = failedInvoice.CatalogItemId,
            PackageId = failedInvoice.PackageId,
            QtyBiotrack = int.Parse(btItem.Remaining_Quantity),
            QtyCova = Convert.ToInt32(catalogItem.QuantityInStock),
            NewQty = Convert.ToInt32(failedInvoice.Quantity) + Convert.ToInt32(catalogItem.QuantityInStock)
        };
        return record;
    }

    private static int CalculateQty(CatalogProductQuantitiesDetailed cova, FailedInvoiceInput inventory)
    {
        return Convert.ToInt32(cova.QuantityInStock) + Convert.ToInt32(inventory.Quantity);
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

