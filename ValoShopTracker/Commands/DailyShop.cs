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

        try
        {


            var account = await _db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == discordId && u.Selected == true);

            if (account != null)
            {
                if (account.TokenExpires > DateTime.UtcNow.AddMinutes(1))
                {
                    return account;
                }

                var rToken = EncryptionHelper.Decrypt(account.EncryptedToken, account.Nonce, account.Tag);

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
        catch (Exception e)
        {
            Console.WriteLine($"error getting account: {e.Message}");
        }

        return new User();

    }


    [SlashCommand("shop", "View your daily valorant shop rotation")]
    public async Task ShopRotation()
    {
        await DeferAsync();

        try
        {
            

            var discId = Context.User.Id;


            var account = await GetAccount(discId);

            if (account == new User())
            {
                Console.WriteLine($"FAILED to get user's account.");
                await FollowupAsync($"{Context.User.Mention} failed getting selected account. Try again later");
            }

            var entitlement = await Auth.GetEntitlement(account.AccessToken);

            if (entitlement == null)
            {
                Console.WriteLine($"Entitlement token GET failed");
                await FollowupAsync($"{Context.User.Mention} error failed getting shop. Try again later.");
            }

            var shop = await Shop.GetStorefront(account.Shard, account.Puuid, entitlement, account.AccessToken);

            if (shop.SkinsPanelLayout.SingleItemOffers.Count == 0)
            {
                throw new Exception($"no items retrieved from daily store request");
            }


            var items = new List<string>();
            var items2 = new List<Embed>();

            for (var i = 0; i < shop.SkinsPanelLayout.SingleItemOffers.Count; i++)
            {
                var info = await Shop.WeaponInfo(shop.SkinsPanelLayout.SingleItemOffers[i]);

                var name = info.data.displayName;

                if (info.status != 200)
                {
                    Console.WriteLine($"GET display name of weapon FAILED");
                    name = "N/A";
                }


                var cost = 0;

                cost = shop.SkinsPanelLayout.SingleItemStoreOffers[i].Cost.ElementAt(0).Value;

                var full = $"{name} - {cost}VP";
                var embed = new EmbedBuilder();

                embed.WithTitle(full).WithImageUrl(info.data.displayIcon);
                items2.Add(embed.Build());
            }

            var hours = shop.SkinsPanelLayout.SingleItemOffersRemainingDurationInSeconds / 60 / 60;

            var min = (shop.SkinsPanelLayout.SingleItemOffersRemainingDurationInSeconds / 60) % 60;

            var seconds = shop.SkinsPanelLayout.SingleItemOffersRemainingDurationInSeconds % 60;

            await FollowupAsync($"Shop rotates in {hours} hours, {min} minutes and {seconds} second/s",
                items2.ToArray());

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting shop rotation: {e.Message}");
            await FollowupAsync($"{Context.User.Mention} error getting shop rotation. Try again later.");
        }
    }
}