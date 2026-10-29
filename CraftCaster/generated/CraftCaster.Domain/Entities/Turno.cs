using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CraftCaster.Domain.Entities;

public partial class Turno
{
    public virtual int Id { get; set; }
    public virtual int Mana { get; set; }
    public virtual Partida Partida { get; set; } = null!;
    public virtual JugadorPartida Jugador { get; set; } = null!;
    public virtual IList<Accion> Acciones { get; set; } = new List<Accion>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Turno other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
