using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager
{
    public class AuthenticationService
    {
        public string HashPassword(string password)
        {
            // Generate a random salt for Argon2id
            byte[] salt = GenerateSalt();
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8; // Number of threads
                argon2.MemorySize = 65536;      // 64 MB
                argon2.Iterations = 4;          // Number of iterations

                byte[] hash = argon2.GetBytes(32); // Generate a 256-bit hash
                return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash);
            }
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('|');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] actualHash = argon2.GetBytes(32);
                return actualHash.SequenceEqual(expectedHash);
            }
        }

        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16]; // 16 bytes = 128 bits
                rng.GetBytes(salt);
                return salt;
            }
        }
    }
}