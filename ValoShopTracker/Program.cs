using System.Collections.Immutable;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using ValoShopTracker;
using Discord.Net;
using Microsoft.Extensions.DependencyInjection;
using ValoShopTracker.DB;
using Microsoft.EntityFrameworkCore;


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
			.AddSingleton(interactionService)
			.AddDbContext<DbConstructor>(options =>
			{
				options.UseSqlite("Data Source=ValoShopTracker.db");
			});

		var serviceProvider = collection.BuildServiceProvider();
		
		_client.InteractionCreated += async (x) =>
		{
			var ctx = new SocketInteractionContext(_client, x);
			var result = await interactionService.ExecuteCommandAsync(ctx, serviceProvider);

			if (!result.IsSuccess)
			{
				if (x is SocketSlashCommand cmd)
				{
					Console.WriteLine($"cmd called: {cmd.Data.Name} FAILED with {result.Error} - {result.ErrorReason}");
				}
				else
				{
					Console.WriteLine($"smth failed: ERR: {result.Error} - {result.ErrorReason}");
				}
			}
			else
			{
				if (x is SocketSlashCommand command)
				{
					Console.WriteLine("cmd called successfully: " + command.Data.Name);
				}
			}



		};

		_client.Log += Log;


		_client.Ready += async () => { await interactionService.RegisterCommandsToGuildAsync(config.GuildId); };

		
		var mod = await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

		
		foreach (var module in mod)
		{
			Console.WriteLine($"Module: {module.Name}");
			foreach (var cmd in module.SlashCommands)
				Console.WriteLine($"  Slash: {cmd.Name}");
			foreach (var cmd in module.ComponentCommands)
				Console.WriteLine($"  Component: {cmd.Name}");
			foreach (var cmd in module.ModalCommands)
				Console.WriteLine($"  Modal: {cmd.Name}");
		}
		
		Console.WriteLine("mods found" + mod.Count());
		
		
		await _client.LoginAsync(TokenType.Bot, config.Token);
		await _client.StartAsync();





		await Task.Delay(-1);

	}
}
