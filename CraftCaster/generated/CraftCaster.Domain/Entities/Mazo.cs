using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class Mazo
{
    public virtual int Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual float CosteMedio { get; set; }
    public virtual int NumCartas { get; set; }
    public virtual EstadoMazo EstadoMazo { get; set; }
    public virtual bool EsPublico { get; set; }
    public virtual float Valoracion { get; set; }
    public virtual byte Imagen { get; set; }
    public virtual string Creador { get; set; } = string.Empty;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual IList<Carta> Cartas { get; set; } = new List<Carta>();
    public virtual IList<Partida> PartidasLocal { get; set; } = new List<Partida>();
    public virtual IList<Partida> PartidasVisitante { get; set; } = new List<Partida>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Mazo other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
