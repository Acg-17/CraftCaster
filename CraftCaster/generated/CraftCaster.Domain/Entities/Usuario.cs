using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CraftCaster.Domain.Entities;

public partial class Usuario
{
    public virtual int Id { get; set; }
    public virtual string NombreUsuario { get; set; } = string.Empty;
    public virtual string Email { get; set; } = string.Empty;
    public virtual string Contrasenia { get; set; } = string.Empty;
    public virtual byte FotoPerfil { get; set; }
    public virtual int Nivel { get; set; }
    public virtual int Exp { get; set; }
    public virtual IList<Carta> Cartas { get; set; } = new List<Carta>();
    public virtual IList<Mazo> Mazos { get; set; } = new List<Mazo>();
    public virtual IList<Partida> PartidasLocal { get; set; } = new List<Partida>();
    public virtual IList<Partida> PartidasVisitante { get; set; } = new List<Partida>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Usuario other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
