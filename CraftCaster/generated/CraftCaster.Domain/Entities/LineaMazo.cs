using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CraftCaster.Domain.Entities;

public partial class LineaMazo
{
    public virtual int Id { get; set; }
    public virtual int Cantidad { get; set; }
    public virtual Mazo Mazo { get; set; } = null!;
    public virtual Carta Carta { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is LineaMazo other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
