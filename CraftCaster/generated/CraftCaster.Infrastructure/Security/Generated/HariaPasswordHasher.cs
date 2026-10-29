using CraftCaster.Application.Abstractions;
using CraftCaster.Infrastructure.Utils;

namespace CraftCaster.Infrastructure.Security;

public sealed class HariaPasswordHasher : IHariaPasswordHasher
{
    public string HashPasswordIfNeeded(string password) =>
        HariaPasswordHashingUtils.HashPasswordIfNeeded(password);

    public string HashPassword(string password) =>
        HariaPasswordHashingUtils.HashPassword(password);

    public bool VerifyPassword(string? storedHash, string? password) =>
        HariaPasswordHashingUtils.VerifyPassword(storedHash, password);
}