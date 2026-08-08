using System.Text.Json.Serialization;

namespace ValoShopTracker.Classes;

public class WeaponInfoResponse
{
    public int status { get; set; }
    [JsonPropertyName("data")]
    public Data2 data { get; set; } 
}


public class Data2
{
    public string displayName { get; set; }
    public string displayIcon { get; set; }
}