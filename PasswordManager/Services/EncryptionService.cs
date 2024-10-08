using System.Security.Cryptography;

namespace PasswordManager;

public class EncryptionService
{
    private readonly Aes _aes;

    public EncryptionService()
    {
        _aes = Aes.Create();
        _aes.KeySize = 256;
    }

    // Method to generate a new Initialization Vector (IV)
    public byte[] GenerateInitializationVector()
    {
        _aes.GenerateIV(); // Generate a new IV
        return _aes.IV;    // Return the generated IV
    }
    
    public byte[] Encrypt(string plainText, byte[] key, byte[] iv)
    {
        using (var encryptor = _aes.CreateEncryptor(key, iv))
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }
    }

    public string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using (var decryptor = _aes.CreateDecryptor(key, iv))
        {
            using (var ms = new MemoryStream(cipherText))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
