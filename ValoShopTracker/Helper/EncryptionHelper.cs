using System.Security.Cryptography;
using System.Text;
using Discord;

namespace ValoShopTracker.Helper;

public class EncryptionHelper
{
    private const string keyEnv = "AES_KEY"; // name of key in env
    
    private static byte[] key => GetKey(keyEnv, 32);

    /// <summary>
    /// gets key from env
    /// </summary>
    /// <param name="env">key name</param>
    /// <param name="size">byte size (recommended 32)</param>
    /// <returns>aes encrypt key</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static byte[] GetKey(string env, int size)
    {
        var val = Environment.GetEnvironmentVariable(env);

        if (string.IsNullOrWhiteSpace(val))
        {
            throw new Exception(
                $"ENV key not found");
            
        }
        
        var keyBytes = Convert.FromBase64String(val); 

        if (keyBytes.Length != size)
            throw new Exception($"ENV key size mismatch");
        

        return keyBytes;
    }


    /// <summary>
    /// aes-256-gcm encrypt
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns>cipertext, nonce and tag</returns>
    public static (byte[], byte[], byte[]) Encrypt(string plainText)
    {

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; 
        RandomNumberGenerator.Fill(nonce);        var pt = Encoding.UTF8.GetBytes(plainText);
        var ct = new byte[pt.Length];
        var tag = new byte[16];
        
        using (var aes = new AesGcm(key, 16))
        {
            aes.Encrypt(nonce, pt, ct, tag);
        }
        
        return (ct, nonce, tag);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="nonce"></param>
    /// <param name="tag"></param>
    /// <returns>returns plain text</returns>
    public static string Decrypt(byte[] cipherText, byte[] nonce, byte[] tag)
    {
        
        byte[] pt = new byte[cipherText.Length];
        using (var aes = new AesGcm(key, 16))
        {
            aes.Decrypt(nonce, cipherText, tag, pt);
        }
        
        return Encoding.UTF8.GetString(pt);
    }
}