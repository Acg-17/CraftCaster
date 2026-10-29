using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CraftCaster.Domain.Entities;

public partial class CartaPartida
{
    public virtual int Id { get; set; }
    public virtual ZonaPartida Zona { get; set; } = null!;
    public virtual Carta Original { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is CartaPartida other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
