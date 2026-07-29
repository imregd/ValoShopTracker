using System.Collections.Immutable;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using ValoShopTracker;
using Discord.Net;
using Microsoft.Extensions.DependencyInjection;


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
	    var config = JsonConvert.DeserializeObject<DiscToken>(File.ReadAllText("appsettings.json"));
        
	    
	    _client = new DiscordSocketClient();
        var interactionService = new InteractionService(_client.Rest);

        var collection = new ServiceCollection()
	        .AddSingleton(_client)
	        .AddSingleton(interactionService);

        var serviceProvider = collection.BuildServiceProvider();

        _client.Ready += async () =>
        {
	        Commands.ClientOnReady(_client);
        };

        _client.InteractionCreated += async (x) =>
        {
	        var ctx = new SocketInteractionContext(_client, x);
	        await interactionService.ExecuteCommandAsync(ctx, serviceProvider);

	        if (x is SocketSlashCommand cmd)
	        {
		        var name = cmd.Data.Name;
		        await x.RespondAsync($"{ctx.User.Mention}, you called {name}");
	        }
        };
        
        _client.Log += Log;
        
        
        _client.Ready += async () =>
        {
	        await interactionService.RegisterCommandsToGuildAsync(config.GuildId);
        };
		
        await _client.LoginAsync(TokenType.Bot, config.Token);
        await _client.StartAsync();
		
        var mod = await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
		
        Console.WriteLine("mods found", mod.Count());


		
        await Task.Delay(-1);
		
    }
}