using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public class HashingPasswords
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }


    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var hashOfInput = HashPassword(password);
        return StringComparer.Ordinal.Compare(hashOfInput, hashedPassword) == 0;
    }


    public static string HashPasswordWithSalt(string password, byte[] salt)
    {
        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000, // 1 hundrend thousand iterations
            numBytesRequested: 256 / 8));

        return hashed;
    }

    public static bool VerifyPasswordWithSalt(string password, string hashedPassword, byte[] salt)
    {
        string hashOfInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 256 / 8));

        return StringComparer.Ordinal.Compare(hashOfInput, hashedPassword) == 0;
    }

    public static byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
    }
}