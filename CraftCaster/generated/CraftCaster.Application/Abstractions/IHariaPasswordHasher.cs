namespace CraftCaster.Application.Abstractions;

public interface IHariaPasswordHasher
{
    string HashPasswordIfNeeded(string password);
    string HashPassword(string password);
    bool VerifyPassword(string? storedHash, string? password);
}