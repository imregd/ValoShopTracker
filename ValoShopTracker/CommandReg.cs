using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace ValoShopTracker;

public class CommandReg
{

    public static async Task ClientOnReady(DiscordSocketClient _client)
    {
        
        var config = JsonConvert.DeserializeObject<DiscToken>(File.ReadAllText("appsettings.json"));

        // global not needed rn, will use when out of dev
        // Console.WriteLine(config.GuildId);
        // var gCommand = new SlashCommandBuilder();
        // gCommand.WithName("first-cmd");
        // gCommand.WithDescription("ts better work yoyo");



        var guildCommand = _client.GetGuild(config.GuildId);
        var guiCommand = new SlashCommandBuilder();
        guiCommand.WithName("frst-guild-cmd");
        guiCommand.WithDescription("ts better work yoyo");
        
        
        try
        {
            
            await guildCommand.CreateApplicationCommandAsync(guiCommand.Build());
            
            // await _client.CreateGlobalApplicationCommandAsync(gCommand.Build());
        }
        catch(HttpException e)
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            
            
            Console.WriteLine(json);
        }
    }
}