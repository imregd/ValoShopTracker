using System.Collections.Immutable;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ValoShopTracker;

public class Program
{

    private static DiscordSocketClient _client;

	
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
	
	
    public static async Task Main()
    {
        _client = new DiscordSocketClient();
		
        _client.Log += Log;
		
        var token = JsonConvert.DeserializeObject<DiscToken>(File.ReadAllText("appsettings.json")).Token;
		
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
		
		
        await Task.Delay(-1);
		
    }
}