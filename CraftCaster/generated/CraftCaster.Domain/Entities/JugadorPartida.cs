using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CraftCaster.Domain.Entities;

public partial class JugadorPartida
{
    public virtual int Id { get; set; }
    public virtual int Vida { get; set; }
    public virtual int NumJugador { get; set; }
    public virtual Partida Partida { get; set; } = null!;
    public virtual IList<ZonaPartida> Zonas { get; set; } = new List<ZonaPartida>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is JugadorPartida other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
