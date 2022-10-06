namespace ConAppJsonParser;

using ConAppJsonParser.Models;
using RestSharp.Authenticators;
using RestSharp;
using static System.Console;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        WriteLine("BioTrack GL Operations \nPreparing data...\n");
        var config = GetConfig(args);
        LogSetup(config.LogFileLocation);
        Helper hp = new(config);

        //Get input data from input File
        var fileToParse = config.FailedAdjFile; //@"C:\temp\JsonProcessing\FailedAdjustments.json";
        var inputExtractData = Helper.GetFailedInvoices(fileToParse);

        //Retrieve All Packages from BioTrack:
        var btInventory = hp.GetBioTrackInventory();

        //Call Catalog for each input item
        foreach (var failedInvoice in inputExtractData)
        {
            Log.Information("Processing Failed Invoice: {failedInvoice}", failedInvoice.InvoiceId);
            var catalogItem = hp.GetCatalogProductQuantitiesDetailed(config.CompanyId, config.LocationId, failedInvoice.CatalogItemId).Result;
            Log.Information("CatalogItemId: {catalogItemId}, Package Id: {packageId}, CovaQty: {covaQty}", catalogItem.Id, 
                catalogItem.Lots.FirstOrDefault().Packages.FirstOrDefault().PackageId, catalogItem.QuantityInStock);

        }

        _ = hp.AdjustPackageQuantity("", "");
        
        WriteLine("Processing FailedAdjustments");

        //Helper.ProcessFailedAdjustments(data, btInventory.Result); 
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

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        //args in case we want to parameterized app input
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configuration) => {
                configuration.Sources.Clear();
                configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            });
    }

}