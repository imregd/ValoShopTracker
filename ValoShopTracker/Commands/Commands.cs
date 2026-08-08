using System.Security.Cryptography;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ValoShopTracker.DB;
using System.Web;
using Microsoft.EntityFrameworkCore;
using ValoShopTracker.Endpoints;
using ValoShopTracker.Helper;

namespace ValoShopTracker;

public class Commands : InteractionModuleBase<SocketInteractionContext>
{

    private readonly DbConstructor _db;
    
    public Commands(DbConstructor context)
    {
        _db = context;
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
            var canConnect = await _db.Database.CanConnectAsync();
            Console.WriteLine($"DB connection successful: {canConnect}");

        }
        catch (Exception e)
        {
            Console.WriteLine($"DB connection failed: {e.Message}");
        }
        
        await RespondAsync($"hey {Context.User.Mention}, ur name is {Context.User.Username} and YOU just called frstgUILDCMD HAHAA also ur a friggin nerd yo");

        

    }


    [SlashCommand("login", "Login with your Riot account to be able to access other commands.")]
    public async Task Login(string name = "")
    {
        var embed = new EmbedBuilder
        {
            Title = "Login",

        };

        embed.AddField("Step 1", $"Click this [URL]({GenerateLoginUrl()})").AddField("Step 2", "Once it throws an error copy and paste the link into the the bot");
        
        var builder = new ComponentBuilder().WithButton("login", $"login-btn:{name}");
        await RespondAsync(embed: embed.Build(), components: builder.Build());
    }


    [ComponentInteraction("login-btn:*")]
    public async Task LoginButton(string name)
    {
        await RespondWithModalAsync<UrlInput>($"receive-token:{name}");
    }


    
    [ModalInteraction("receive-token:*")]
    public async Task ReceiveToken(string name, UrlInput input)
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
        var userId = Context.User.Id;

        var tokens = await Auth.GetTokens(code);
        var pInfo = await Auth.PlayerInfoRequest(tokens.AccessToken);

        
        
        var userInfo = await _db.Users.FirstOrDefaultAsync(u => u.Puuid == pInfo.Puuid && u.DiscordUserId == userId);

        

        if (userInfo != null)
        {
            await RespondAsync($"{Context.User.Mention}. Account already exists with the name (if added): {userInfo.Name}");
        }

        
        try
        {
            var encryptedToken = EncryptionHelper.Encrypt(tokens.RefreshToken);
            
            
            var activeAccount = await _db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == userId && u.Selected);

            if (activeAccount != null)
            {
                activeAccount.Selected = false;
            }
            
            var user = new User
            {
                DiscordUserId = userId,
                Puuid = pInfo.Puuid,
                EncryptedToken = encryptedToken.Item1,
                Nonce = encryptedToken.Item2,
                Tag = encryptedToken.Item3,
                Shard = pInfo.Affinity.Region,
                AccessToken = tokens.AccessToken,
                TokenExpires = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn),
                Name =  name,
                Selected =  true
            };
        
            await  _db.Users.AddAsync(user);
            
            
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
        }

        
        

        
        await RespondAsync($"{Context.User.Mention} account successfully logged in, you can call /shop or any other commands now!");
    }


    [SlashCommand("view-accounts",
        "View accounts associated to your account and its ID which would be used to delete accounts")]
    public async Task ViewAccounts()
    {
        var accounts = await _db.Users.Where(u => u.DiscordUserId ==  Context.User.Id).ToListAsync();


        var embed = new EmbedBuilder().WithTitle("Your accounts").WithDescription("Use **ID** when selecting to deleting").WithColor(Color.Blue);
        
        

        if (accounts.Count == 0)
        {
            embed.WithDescription("You don't have any accounts linked.");
        }
        else
        {
            var accountList = string.Join("\n",
                accounts.Select((account, index) =>
                    $"`{index + 1,2}` •   **{account.Name}** •   `{account.Shard}`" +
                    (account.Selected ? "   (**SELECTED**)" : "")));

            embed.AddField("Accounts", accountList);
        }
        
        await RespondAsync(embed: embed.Build());
    }


    [SlashCommand("delete-account", "Delete accounts associated to your discord account")]
    public async Task DeleteAccount(int Id)
    {
        var accounts = await _db.Users.Where(u => u.DiscordUserId == Context.User.Id).ToListAsync();

        try
        {


            var accToDelete = accounts[Id - 1];


            if (accToDelete.Selected)
            {
                var newSelected = accounts.Where(u => u.Id != accToDelete.Id).FirstOrDefault();

                if (newSelected != null)
                {
                    newSelected.Selected = false;

                }

            }
            

            _db.Users.Remove(accToDelete);
            await _db.SaveChangesAsync();

            await RespondAsync($"{Context.User.Mention} The account with name: {accToDelete.Name} has been deleted");

        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            await RespondAsync($"{Context.User.Mention} account not found. Are you sure you entered the ID correctly?");

        }
    }

    [SlashCommand("select-account", "Select an account to use.")]
    public async Task SelectAccount(int Id)
    {
        var account = await _db.Users.Where(u => u.DiscordUserId ==  Context.User.Id).ToListAsync();
        
        var wantedAccount = account.FirstOrDefault(a => a.Id == Id);
        var unselectedAccount = account.FirstOrDefault(a => a.Selected);
        if (wantedAccount != null)
        {
            wantedAccount.Selected = true;
        }

        if (unselectedAccount != null)
        {
            unselectedAccount.Selected = false;
        }
        
        await _db.SaveChangesAsync();

        await RespondAsync($"{Context.User.Mention} Account {Id} is now selected!");
    }

[SlashCommand("fix-acc-debug", "fix-debug")]
    public  async Task FixAccounts()
    {
        var accounts = await _db.Users.Where(u => u.Selected).ToListAsync();

        var debug = new EmbedBuilder();


        var i = 0;
        foreach (var account in accounts)
        {
            debug.AddField($"account index", i);
            i++;
        }
        
        await RespondAsync(embed: debug.Build());
        
        
    }
    
    
}