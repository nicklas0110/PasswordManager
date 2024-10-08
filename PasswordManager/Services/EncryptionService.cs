using System.Security.Cryptography;
using System.IO;

namespace PasswordManager.Services
{
    public class EncryptionService
    {
        private readonly Aes _aes;

        public EncryptionService()
        {
            _aes = Aes.Create();
            _aes.KeySize = 256;
            _aes.Padding = PaddingMode.PKCS7; // Set padding explicitly to ensure consistency
        }

        public byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            _aes.Key = key;
            _aes.IV = iv;

            using (var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV))
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Flush(); // Ensure everything is written to the CryptoStream
                cs.FlushFinalBlock(); // Finalize the encryption
                return ms.ToArray();
            }
        }

        public string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            try
            {
                _aes.Key = key;
                _aes.IV = iv;

                using (var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV))
                using (var ms = new MemoryStream(cipherText))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("Decryption failed. The key or IV might be incorrect, or the data may be corrupted.");
            }
        }

        // Generate a new Initialization Vector (IV)
        public byte[] GenerateInitializationVector()
        {
            _aes.GenerateIV(); // Generate a new IV
            return _aes.IV;    // Return the generated IV
        }
    }
}
