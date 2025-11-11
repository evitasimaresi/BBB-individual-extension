using System.Diagnostics;
using System.Security.Cryptography;

namespace BBB.Services;

public static class PBKDF2Hasher
{
    public static byte[] Hash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);

    public static bool Verify(string password, string hashString, string saltString)
    {
        Debug.WriteLine(hashString);
        byte[] hash = Convert.FromBase64String(hashString);
        byte[] salt = Convert.FromBase64String(saltString);

        var computed = Hash(password, salt);
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}
