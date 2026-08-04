using System.Text.Json.Serialization;

namespace ValoShopTracker.Classes;

public class PlayerInfo
{
    [JsonPropertyName("sub")]
    public string Puuid { get; set; }
    
    [JsonPropertyName("affinity")]
    public Affinity Affinity { get; set; }
}


public class Affinity
{
    [JsonPropertyName("pp")]
    public string Region { get; set; }
}