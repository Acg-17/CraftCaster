using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class Partida
{
    public virtual int Id { get; set; }
    public virtual EstadoPartida Estado { get; set; }
    public virtual Usuario Usuario1 { get; set; } = null!;
    public virtual Usuario? Usuario2 { get; set; }
    public virtual Mazo Mazo1 { get; set; } = null!;
    public virtual Mazo? Mazo2 { get; set; }
    public virtual IList<Turno> Turnos { get; set; } = new List<Turno>();
    public virtual IList<JugadorPartida> Jugadores { get; set; } = new List<JugadorPartida>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Partida other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
