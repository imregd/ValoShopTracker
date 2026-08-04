using System.Net;
using System.Text.Json;
using RestSharp;
using ValoShopTracker.Classes;


namespace ValoShopTracker.Endpoints;

public class Shop
{
    public static async Task<StorefrontResponse> GetStorefront(string shard, string puuid, string entitlement, string accessToken)
    {
        
        
        string clientVer = await GetClientVersion();
        string platformVer =
            "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9";
        var client = new RestClient($"https://pd.{shard}.a.pvp.net/store/v3/storefront/{puuid}");

        var request = new RestRequest("", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody("{}");
        request.AddHeader("Authorization", $"Bearer {accessToken}");
        request.AddHeader("X-Riot-Entitlements-Jwt", entitlement);
        request.AddHeader("X-Riot-ClientPlatform", platformVer);
        request.AddHeader("X-Riot-ClientVersion", clientVer);
        var response = await client.ExecuteAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<StorefrontResponse>(response.Content);
        }
        
        Console.WriteLine($"Storefront request failed: {response.StatusCode} - {response.Content}");
        Console.WriteLine($"parameters in method: shard: {shard}, puuid: {puuid},  entitlement: {entitlement}, platformVer: {platformVer}, clientVer: {clientVer}, accessToken: {accessToken}");
        return new StorefrontResponse();
    }


    public static async Task<string> GetClientVersion()
    {
        var client = new RestClient("https://valorant-api.com/v1/version");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var info = JsonSerializer.Deserialize<ClientVersionResponse>(response.Content);
            return info.data.riotClientVersion;
        }
        else
        {
            return string.Empty;
        }
    }
    
    
    
}