using System.Security.Cryptography;
using System.Text;
using Discord;

namespace ValoShopTracker.Helper;

public class EncryptionHelper
{
    private const string keyEnv = "AES_KEY";
    
    private static byte[] key => GetKey(keyEnv, 32);

    private static byte[] GetKey(string env, int size)
    {
        var val = Environment.GetEnvironmentVariable(env);

        if (string.IsNullOrWhiteSpace(val))
        {
            throw new InvalidOperationException(
                $"env var not set OR not same size");
            
        }
        
        var keyBytes = Convert.FromBase64String(val); 

        if (keyBytes.Length != size)
            throw new InvalidOperationException($"{env} is {keyBytes.Length} bytes, expected {size}.");
        

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