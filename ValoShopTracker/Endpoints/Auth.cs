using System.Net;
using System.Text.Json;
using RestSharp;
using ValoShopTracker.Classes;

namespace ValoShopTracker.Endpoints;

public class Auth
{
    public static async Task<PlayerInfo> PlayerInfoRequest(string token)
    {
        var client = new RestClient("https://auth.riotgames.com/userinfo");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", $"Bearer {token}");
        var response = await client.ExecuteAsync(request);

        if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
        {
            Console.WriteLine(response.StatusCode);
            return new PlayerInfo();
        }

        var info = JsonSerializer.Deserialize<PlayerInfo>(response.Content);

        return info;
    }

    public static async Task<TokenResponse> GetTokens(string code)
    {
        var client = new RestClient("https://auth.riotgames.com/token");
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("grant_type", "authorization_code");
        request.AddParameter("code", code);
        request.AddParameter("redirect_uri", "http://localhost/redirect");
        request.AddParameter("client_id", "riot-client");

        var response = await client.ExecuteAsync(request);
        
        if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
        {
            Console.WriteLine(response.StatusCode);
            return new TokenResponse();
        }
        
        var token = JsonSerializer.Deserialize<TokenResponse>(response.Content);
        return token;
    }
}