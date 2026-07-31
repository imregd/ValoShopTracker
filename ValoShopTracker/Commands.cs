using Discord.Interactions;
using Discord.WebSocket;
using ValoShopTracker.DB;

namespace ValoShopTracker;

public class Commands : InteractionModuleBase<SocketInteractionContext>
{

    private readonly DbConstructor _context;
    
    public Commands(DbConstructor context)
    {
        _context = context;
    }

    [SlashCommand("frst-guild-cmd", "ts better work yoyo")]
    public async Task FrstGuildCmd()
    {
        for (int i = 0; i < 5
             
             
             ; i++)
        {
            await Context.Channel.SendMessageAsync("https://klipy.com/gifs/homer-let-the-barts-out-2");
        }

        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            Console.WriteLine($"DB connection successful: {canConnect}");

        }
        catch (Exception e)
        {
            Console.WriteLine($"DB connection failed: {e.Message}");
        }
        
        await RespondAsync($"hey {Context.User.Mention}, ur name is {Context.User.Username} and YOU just called frstgUILDCMD HAHAA also ur a friggin nerd yo");

        

    }
    
}