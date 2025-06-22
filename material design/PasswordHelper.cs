using System;
using System.Security.Cryptography;

public static class PasswordHelper
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public static (string Hash, string Salt) GenerateHash(string password)
    {
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        byte[] hashBytes;
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations))
        {
            hashBytes = pbkdf2.GetBytes(HashSize);
        }

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(storedHash))  return false;

        byte[] saltBytes = Convert.FromBase64String(storedSalt);
        byte[] hashBytes = Convert.FromBase64String(storedHash);

        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, saltBytes, Iterations))
        {
            byte[] enteredHashBytes = pbkdf2.GetBytes(HashSize);
            return SlowEquals(enteredHashBytes, hashBytes);
        }
    }

    private static bool SlowEquals(byte[] a, byte[] b)
    {
        uint diff = (uint)a.Length ^ (uint)b.Length;
        for (int i = 0; i < a.Length && i < b.Length; i++)
            diff |= (uint)(a[i] ^ b[i]);
        return diff == 0;
    }
}