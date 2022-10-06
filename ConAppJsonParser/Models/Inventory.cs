
using System.Text.Json.Serialization;

namespace ConAppJsonParser.Models;

public class Inventory
{
    public int Success { get; set; }
    [JsonPropertyName("inventory")]
    public List<InventoryItem> InventoryItems { get; set; }
}

	public class InventoryItem
	{
    [JsonPropertyName("is_sample")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? Is_Sample { get; set; }  //"is_sample":null,
    
		[JsonPropertyName("inventoryparentid")]
    public IList<string> InventoryParentId { get; set; }  //inventoryparentid":null,
        
		[JsonPropertyName("sessiontime")]
    public int SessionTime { get; set; }  //sessiontime":1592256324,
	
		[JsonPropertyName("deleted")]
    public int Deleted { get; set; }  //deleted":0,

    [JsonPropertyName("plantid")]
    public List<string> PlantId { get; set; }  //plantid":["0060344052789152"],

    [JsonPropertyName("wet")]
    public int Wet { get; set; }  //wet":0,

    [JsonPropertyName("usable_weight")]
    public string Usable_Weight { get; set; }  //usable_weight":"233.00",

    [JsonPropertyName("parentid")]
    public IList<string> ParentId { get; set; }  //parentid":null,

    [JsonPropertyName("seized")]
    public string Seized { get; set; } //seized":null,

    [JsonPropertyName("id")]
    public string Id { get; set; } //id":"9506928910693842",

    [JsonPropertyName("inventorystatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? InventoryStatus { get; set; } //inventorystatus":null,

    [JsonPropertyName("location")]
    public string Location { get; set; } //location":"45",

    [JsonPropertyName("rec_usableweight")]
    public string Rec_UsableWeight { get; set; }  //rec_usableweight":null,

    [JsonPropertyName("productname")]
    public string ProductName { get; set; }  //productname":null,

    [JsonPropertyName("source_id")]
    public string Source_Id { get; set; }  //source_id":null,

    //[JsonPropertyName("inventorystatustime")]
    //public string InventoryStatusTime { get; set; }  //inventorystatustime":null,

    [JsonPropertyName("remaining_quantity")]
    public string Remaining_Quantity { get; set; }  //remaining_quantity":"0",

    [JsonPropertyName("transactionid_original")]
    public int TransactionId_Original { get; set; }  //transactionid_original":30212419,

    [JsonPropertyName("inventorytype")]
    public int InventoryType { get; set; }  //inventorytype":6,

    [JsonPropertyName("transactionid")]
    public int TransactionId { get; set; }  //transactionid":30212558,

    [JsonPropertyName("net_package")]
    public string Net_Package { get; set; }  //net_package":null,

    [JsonPropertyName("strain")]
    public string Strain { get; set; }  //strain":"CYC - Cherry Cake"

    //[JsonPropertyName("currentroom")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //public int? CurrentRoom { get; set; }
}
