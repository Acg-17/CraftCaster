using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Domain.Entities;

public partial class Accion
{
    public virtual int Id { get; set; }
    public virtual TipoAccion Name { get; set; }
    public virtual Turno Turno { get; set; } = null!;
    public virtual CartaPartida? Origen { get; set; }
    public virtual IList<CartaPartida> Objetivo { get; set; } = new List<CartaPartida>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Accion other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
