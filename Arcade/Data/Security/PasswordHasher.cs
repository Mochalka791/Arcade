using System.Security.Cryptography;
using System.Text;

namespace Arcade.Data.Security;

public class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 150_000;

    public void CreateHash(string password, out byte[] hash, out byte[] salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        salt = RandomNumberGenerator.GetBytes(SaltSize);
        hash = PBKDF2(password, salt);
    }

    public bool Verify(string password, byte[] salt, byte[] expectedHash)
    {
        if (string.IsNullOrEmpty(password) || salt.Length == 0 || expectedHash.Length == 0)
        {
            return false;
        }

        var computed = PBKDF2(password, salt);
        return CryptographicOperations.FixedTimeEquals(computed, expectedHash);
    }

    private static byte[] PBKDF2(string password, byte[] salt)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(KeySize);
    }
}
