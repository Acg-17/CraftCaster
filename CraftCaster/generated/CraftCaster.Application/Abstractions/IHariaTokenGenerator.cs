namespace CraftCaster.Application.Abstractions;

public sealed record HariaTokenDescriptor(
    string Subject,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyDictionary<string, string?> Claims);

public interface IHariaTokenGenerator
{
    TimeSpan DefaultLifetime { get; set; }
    string CreateToken(string subject, IReadOnlyDictionary<string, string?> claims);
    string CreateToken(HariaTokenDescriptor descriptor);
    bool TryValidateToken(string token, out HariaTokenDescriptor? descriptor);
}