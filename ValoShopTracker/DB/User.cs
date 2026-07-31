namespace ValoShopTracker.DB;

public class User
{
    public int UserId { get; set; }
    public byte[] EncryptedToken { get; set; }
    public byte[] Nonce { get; set; }
    public byte[] Tag { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}