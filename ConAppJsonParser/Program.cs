using ConAppJsonParser.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestSharp;
using Serilog;
using static System.Console;

namespace ConAppJsonParser;

public class Program
{
    public static void Main(string[] args)
    {
        WriteLine("BioTrack GL Operations \nPreparing data...\n");
        var config = GetConfig(args);
        LogSetup(config.LogFileLocation);
        Helper hp = new(config);

        //Get input data from input File
        var inputExtractData = Helper.GetFailedInvoices(config.FailedAdjFile);

        //Get BioTrack All Inventory including inactive items
        var btInventory = hp.GetBioTrackInventory().Result;

        List<AdjustmentReport> report = new();
        foreach (var failedInvoice in inputExtractData)
        {
            Log.Information("Processing Failed Invoice: {failedInvoice}", failedInvoice.InvoiceId);
            var catalogItem = hp.GetCatalogProductQuantitiesDetailed(config.CompanyId, config.LocationId, failedInvoice.CatalogItemId).Result;
            
            var covaPackageId = catalogItem.Lots.FirstOrDefault().Packages.Where(x => x.PackageId == failedInvoice.PackageId);
            Log.Information("CatalogItemId: {catalogItemId}, Package Id: {covaPackageId}, CovaQty: {covaQty}", 
                catalogItem.Id, covaPackageId, catalogItem.QuantityInStock);
            
            var result = hp.AdjustPackageQuantity(failedInvoice, catalogItem).Result;
            var btItem = btInventory.InventoryItems.FirstOrDefault(x => x.Id == failedInvoice.PackageId);
            if (btItem is null)
            {
                Log.Error("Item with package ID: {packageId} not found in Item: {@btItem}", failedInvoice.PackageId, btItem);
                continue;
            }
            report.Add(Helper.GenerateReportRecord(failedInvoice, catalogItem, btItem));
        }

        Log.Information("Number of Adjustment to report: {reportNumber}", report.Count);
        Helper.SaveToCSV(report);
        
        Log.Information("Finished Adjustment Cycle.");
    }

    private static ProcessingInput GetConfig(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        IConfiguration configBase = host.Services.GetRequiredService<IConfiguration>();
        var config = configBase.GetSection("ProcessingInput");
        return config.Get<ProcessingInput>();
    }

    private static void LogSetup(string file) 
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(file, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        //args in case we want to parameterized app input
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configuration) => {
                configuration.Sources.Clear();
                configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                configuration.AddUserSecrets<Program>();
            });
    }
}