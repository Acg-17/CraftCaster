using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class Habilidad
{
    public virtual int Id { get; set; }
    public virtual TipoHabilidad Tipo { get; set; }
    public virtual int Cantidad { get; set; }
    public virtual TipoObjetivo TipoObjetivo { get; set; }
    public virtual int Objetivo { get; set; }
    public virtual Estado Estado { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Habilidad other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
