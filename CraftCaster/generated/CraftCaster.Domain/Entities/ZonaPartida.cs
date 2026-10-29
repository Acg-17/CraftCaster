using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class ZonaPartida
{
    public virtual int Id { get; set; }
    public virtual TipoZona TipoZona { get; set; }
    public virtual JugadorPartida Jugador { get; set; } = null!;
    public virtual IList<CartaPartida> Cartas { get; set; } = new List<CartaPartida>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ZonaPartida other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
