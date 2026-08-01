using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ValoShopTracker.DB;
using System.Web;

namespace ValoShopTracker;

public class Commands : InteractionModuleBase<SocketInteractionContext>
{

    private readonly DbConstructor _context;
    
    public Commands(DbConstructor context)
    {
        _context = context;
    }


    public string GenerateLoginUrl()
    {
        var baseUrl =
            "https://auth.riotgames.com/authorize?client_id=riot-client&redirect_uri=http%3A%2F%2Flocalhost%2Fredirect&response_type=code&scope=openid+link+ban+lol_region+account+offline_access&nonce=";
        
        string nonce = Guid.NewGuid().ToString("N");
        return $"{baseUrl}{nonce}";
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


    [SlashCommand("login", "Login with your Riot account to be able to access other commands.")]
    public async Task Login()
    {
        var builder = new ComponentBuilder().WithButton("login", "login-btn");
        await RespondAsync($"{Context.User.Mention} yo son wyw i aint ready yet CHILLAX, URL: {GenerateLoginUrl()}", components: builder.Build());
    }


    [ComponentInteraction("login-btn")]
    public async Task LoginButton()
    {
        await RespondWithModalAsync<UrlInput>("receive-token");
    }


    
    [ModalInteraction("receive-token")]
    public async Task ReceiveToken(UrlInput input)
    {
        var uri = new Uri(input.Url);

        var pieces = uri.Query.Trim('?').Split("&");

        string code = "";
        
        foreach (var piece in pieces)
        {
            var split = piece.Split('=');
            if (split.Length == 2)
            {
                if (split[0] == "code")
                {
                    code = Uri.UnescapeDataString(split[1]);
                }
            }
        }
        
        
        await RespondAsync($"{Context.User.Mention} wow it actualy workedl ezgo. CODE: {code}, also user id: {Context.User.Id}");
    }
}