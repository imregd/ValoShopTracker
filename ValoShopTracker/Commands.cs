using Discord.Interactions;
using Discord.WebSocket;

namespace ValoShopTracker;

public class Commands : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("frst-guild-cmd", "ts better work yoyo")]
    public async Task FrstGuildCmd()
    {
        for (int i = 0; i < 100; i++)
        {
            await Context.Channel.SendMessageAsync("https://klipy.com/gifs/homer-let-the-barts-out-2");
        }
        await RespondAsync($"hey {Context.User.Mention}, ur name is {Context.User.Username} and YOU just called frstgUILDCMD HAHAA also ur a friggin nerd yo");

    }
    
}