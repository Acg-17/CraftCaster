using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class Carta
{
    public virtual int Id { get; set; }
    public virtual string Nombre { get; set; } = string.Empty;
    public virtual Rareza Rareza { get; set; }
    public virtual int CosteMana { get; set; }
    public virtual TipoCarta Tipo { get; set; }
    public virtual int? Vida { get; set; }
    public virtual int? Ataque { get; set; }
    public virtual string Descripcion { get; set; } = string.Empty;
    public virtual byte Imagen { get; set; }
    public virtual bool EsPublica { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual IList<Estado> Estados { get; set; } = new List<Estado>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Carta other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
