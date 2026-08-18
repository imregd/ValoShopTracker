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


    /// <summary>
    /// generates a login url to be used, gives us the token to let us get refresh + access token
    /// </summary>
    /// <returns></returns>
    public string GenerateLoginUrl()
    {
        var baseUrl =
            "https://auth.riotgames.com/authorize?client_id=riot-client&redirect_uri=http%3A%2F%2Flocalhost%2Fredirect&response_type=code&scope=openid+link+ban+lol_region+account+offline_access&nonce=";
        
        string nonce = Guid.NewGuid().ToString("N");
        return $"{baseUrl}{nonce}";
    }
    


    /// <summary>
    /// start of login
    /// </summary>
    /// <param name="name"></param>
    [SlashCommand("login", "Login with your Riot account to be able to access other commands.")]
    public async Task Login(string name = "none")
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

        if (name == "none")
        {
            name = "";
        }

        try
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


            if (String.IsNullOrEmpty(code))
            {
                await RespondAsync(
                    $"{Context.User.Mention}. Code could not be retrieved from URL, did you paste it in correctly?");

                return;
            }

            var userId = Context.User.Id;

            var tokens = await Auth.GetTokens(code);
            var pInfo = await Auth.PlayerInfoRequest(tokens.AccessToken);


            var userInfo =
                await _db.Users.FirstOrDefaultAsync(u => u.Puuid == pInfo.Puuid && u.DiscordUserId == userId);







            var encryptedToken = EncryptionHelper.Encrypt(tokens.RefreshToken);


            if (encryptedToken.Item1.Length == 0)
            {
                throw new Exception("encrypt failed");
            }

            var activeAccount = await _db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == userId && u.Selected);

            if (activeAccount != null)
            {
                activeAccount.Selected = false;
            }


            if (userInfo == null)
            {


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
                    Name = name,
                    Selected = true
                };

                await _db.Users.AddAsync(user);
                
                await _db.SaveChangesAsync();

                
                await RespondAsync(
                    $"{Context.User.Mention} account successfully logged in, you can call /shop or any other commands now!");

                return;
            }

            userInfo.EncryptedToken = encryptedToken.Item1;
            userInfo.Nonce = encryptedToken.Item2;
            userInfo.Tag = encryptedToken.Item3;
            userInfo.AccessToken =  tokens.AccessToken;
            userInfo.TokenExpires = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
            
            await _db.SaveChangesAsync();


            await RespondAsync($"{Context.User.Mention} account already exists, token refreshed for account");

            


        }
        catch (Exception e)
        {
            Console.WriteLine($"error logging in: {e.Message}");
            await RespondAsync($"{Context.User.Mention} Logging in failed. Try again later.");
        }
        
        }


    [SlashCommand("view-accounts",
        "View accounts associated to your account and its ID which would be used to delete accounts")]
    public async Task ViewAccounts()
    {
        try
        {
            var accounts = await _db.Users.Where(u => u.DiscordUserId == Context.User.Id).ToListAsync();


            var embed = new EmbedBuilder().WithTitle("Your accounts")
                .WithDescription("Use **ID** when selecting to deleting").WithColor(Color.Blue);



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
        catch (Exception e)
        {
            Console.WriteLine($"error viewing accounts: {e.Message}");
        }
        
    }


    [SlashCommand("delete-account", "Delete accounts associated to your discord account")]
    public async Task DeleteAccount(int Id)
    {

        try
        {

            var accounts = await _db.Users.Where(u => u.DiscordUserId == Context.User.Id).ToListAsync();


            if (Id < 1 || Id > accounts.Count)
            {
                await RespondAsync($"{Context.User.Mention} account not found.");
                return;
            }

            var accToDelete = accounts[Id - 1];


            if (accToDelete.Selected)
            {
                var newSelected = accounts.Where(u => u.Id != accToDelete.Id).FirstOrDefault();

                if (newSelected != null)
                {
                    newSelected.Selected = true;

                }

            }
            

            _db.Users.Remove(accToDelete);
            await _db.SaveChangesAsync();

            await RespondAsync($"{Context.User.Mention} The account with name: {accToDelete.Name} has been deleted");

        }
        catch (Exception e)
        {
            Console.WriteLine($"error deleting account: {e.Message}");
            await RespondAsync($"{Context.User.Mention} Error deleting account. Try again later");

        }
    }

    [SlashCommand("select-account", "Select an account to use.")]
    public async Task SelectAccount(int Id)
    {
        try
        {
            var accounts = await _db.Users.Where(u => u.DiscordUserId == Context.User.Id).ToListAsync();

            if (Id < 1 || Id > accounts.Count)
            {
                await RespondAsync($"{Context.User.Mention} Account not found.");
                return;
            }

            var wantedAccount = accounts[Id - 1];
            var unselectedAccount = accounts.FirstOrDefault(a => a.Selected);

            wantedAccount.Selected = true;

            if (unselectedAccount != null && unselectedAccount.Id != wantedAccount.Id)
            {
                unselectedAccount.Selected = false;
            }

            await _db.SaveChangesAsync();

            await RespondAsync($"{Context.User.Mention} Account {wantedAccount.Name} is now selected!");
        }
        catch (Exception e)
        {
            Console.WriteLine($"error selecting account: {e.Message}");
            await RespondAsync($"{Context.User.Mention} Error selecting account");
        }
    }
    
}