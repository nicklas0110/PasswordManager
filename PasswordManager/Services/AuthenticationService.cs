using System.Security.Cryptography;

namespace PasswordManager;

public class AuthenticationService
{
    private const int Iterations = 10000;

    public byte[] DeriveKey(string password, byte[] salt)
    {
        using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, Iterations))
        {
            return rfc2898.GetBytes(32); // 256-bit key
        }
    }
}
