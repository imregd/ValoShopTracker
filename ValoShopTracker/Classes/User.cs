namespace ValoShopTracker.DB;

public class User
{
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string Puuid { get; set; }
    public byte[] EncryptedToken { get; set; }
    public byte[] Nonce { get; set; }
    public byte[] Tag { get; set; }
    public string Shard { get; set; }
    public string? Name { get; set; }
    
    public string AccessToken { get; set; }
    
    public DateTime TokenExpires { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}