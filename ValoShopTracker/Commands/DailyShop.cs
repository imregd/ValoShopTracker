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

public class DailyShop : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DbConstructor _db;
    
    public DailyShop(DbConstructor db)
    {
        _db = db;
    }



    public async Task<User> GetAccount(ulong discordId)
    {
        var account = await _db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == discordId && u.Selected == true);

        if (account != null)
        {
            if (account.TokenExpires > DateTime.UtcNow.AddMinutes(1))
            {
                Console.WriteLine("new token not needed");
                return account;
            }
            var rToken =  EncryptionHelper.Decrypt(account.EncryptedToken, account.Nonce, account.Tag);
            
            var info = await Auth.RefreshTokens(rToken);

            var newToken = EncryptionHelper.Encrypt(info.RefreshToken);
            
            

            account.TokenExpires = DateTime.UtcNow.AddSeconds(info.ExpiresIn);
            account.AccessToken = info.AccessToken;
            account.EncryptedToken = newToken.Item1;
            account.Nonce = newToken.Item2;
            account.Tag = newToken.Item3;
            
            await _db.SaveChangesAsync();

            Console.WriteLine("new token success");
            return account;


        }

        return null;
        
    }


    [SlashCommand("shop", "View your daily valorant shop rotation")]
    public async Task ShopRotation()
    {
        await DeferAsync();
        
        var discId = Context.User.Id;
         
        
        var account = await GetAccount(discId);
        
        var entitlement = await Auth.GetEntitlement(account.AccessToken);

        if (entitlement == null)
        {
            await FollowupAsync("bro it failed, entitlement req didnt work mb");
        }
        
        var shop = await Shop.GetStorefront(account.Shard, account.Puuid, entitlement, account.AccessToken);

        var items = new List<string>();
        var items2 = new List<Embed>();

        for(var i = 0; i < shop.SkinsPanelLayout.SingleItemOffers.Count; i++)
        {
            var info = await Shop.WeaponInfo(shop.SkinsPanelLayout.SingleItemOffers[i]);

            var cost = 0;
            try
            {
                cost = shop.SkinsPanelLayout.SingleItemStoreOffers[i].Cost.ElementAt(0).Value;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            var full = $"{info.data.displayName} - {cost}VP";
            var embed = new EmbedBuilder();

            embed.WithTitle(full).WithImageUrl(info.data.displayIcon);
            items2.Add(embed.Build());
        }
        
        Console.WriteLine("we got da shop baby");
        var hours = shop.SkinsPanelLayout.SingleItemOffersRemainingDurationInSeconds / 60 / 60;

        var min = (shop.SkinsPanelLayout.SingleItemOffersRemainingDurationInSeconds / 60 ) % 60;
        
        var seconds = shop.SkinsPanelLayout.SingleItemOffersRemainingDurationInSeconds % 60;
        
        await FollowupAsync($"Shop rotates in {hours} hours, {min} minutes and {seconds} second/s", items2.ToArray());
    }
}