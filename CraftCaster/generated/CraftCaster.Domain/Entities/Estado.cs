using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class Estado
{
    public virtual int Id { get; set; }
    public virtual TipoEstado Tipo { get; set; }
    public virtual IList<Habilidad> Habilidades { get; set; } = new List<Habilidad>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Estado other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
