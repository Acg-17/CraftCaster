using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CraftCaster.Application.Abstractions;

namespace CraftCaster.Infrastructure.Security;

public sealed class HariaTokenGenerator : IHariaTokenGenerator
{
    private const string SecretEnvironmentVariable = "HARIA_TOKEN_SECRET";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public TimeSpan DefaultLifetime { get; set; } = TimeSpan.FromHours(1);

    public string CreateToken(string subject, IReadOnlyDictionary<string, string?> claims)
    {
        var issuedAtUtc = DateTimeOffset.UtcNow;
        return CreateToken(new HariaTokenDescriptor(
            subject,
            issuedAtUtc,
            issuedAtUtc.Add(DefaultLifetime),
            claims));
    }

    public string CreateToken(HariaTokenDescriptor descriptor)
    {
        var payload = new HariaTokenPayload
        {
            Subject = descriptor.Subject,
            IssuedAtUtc = descriptor.IssuedAtUtc,
            ExpiresAtUtc = descriptor.ExpiresAtUtc,
            Claims = new Dictionary<string, string?>(descriptor.Claims, StringComparer.Ordinal)
        };

        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
        var payloadPart = Base64UrlEncode(payloadBytes);
        var signaturePart = Base64UrlEncode(Sign(payloadPart));
        return payloadPart + "." + signaturePart;
    }

    public bool TryValidateToken(string token, out HariaTokenDescriptor? descriptor)
    {
        descriptor = null;
        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        var expectedSignature = Sign(parts[0]);
        byte[] suppliedSignature;
        try
        {
            suppliedSignature = Base64UrlDecode(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, suppliedSignature))
        {
            return false;
        }

        HariaTokenPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<HariaTokenPayload>(Base64UrlDecode(parts[0]), JsonOptions);
        }
        catch (JsonException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }

        if (payload is null || payload.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        descriptor = new HariaTokenDescriptor(
            payload.Subject,
            payload.IssuedAtUtc,
            payload.ExpiresAtUtc,
            payload.Claims);
        return true;
    }

    private static byte[] Sign(string payloadPart)
    {
        using var hmac = new HMACSHA256(GetSecret());
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadPart));
    }

    private static byte[] GetSecret()
    {
        var secret = Environment.GetEnvironmentVariable(SecretEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(secret))
        {
            secret = "development-only-change-HARIA_TOKEN_SECRET";
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }

    private sealed class HariaTokenPayload
    {
        public string Subject { get; set; } = "";
        public DateTimeOffset IssuedAtUtc { get; set; }
        public DateTimeOffset ExpiresAtUtc { get; set; }
        public Dictionary<string, string?> Claims { get; set; } = new(StringComparer.Ordinal);
    }
}