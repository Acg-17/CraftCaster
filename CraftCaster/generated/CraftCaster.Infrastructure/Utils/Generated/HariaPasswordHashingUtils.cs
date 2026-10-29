using System.Security.Cryptography;

namespace CraftCaster.Infrastructure.Utils;

public static class HariaPasswordHashingUtils
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 600000;

    public static string HashPasswordIfNeeded(string password) =>
        IsPasswordHash(password) ? password : HashPassword(password);

    public static string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            ".",
            Algorithm,
            Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static bool VerifyPassword(string? storedHash, string? password)
    {
        if (string.IsNullOrWhiteSpace(storedHash) || password is null)
        {
            return false;
        }

        var parts = storedHash.Split('.');
        if (parts.Length != 4 || parts[0] != Algorithm || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsPasswordHash(string value) =>
        value.StartsWith(Algorithm + ".", StringComparison.Ordinal);
}