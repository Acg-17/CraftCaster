using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Usuario_new) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class UsuarioService
{
    public async Task<Usuario> NewAsync(string p_nombreUsuario, string p_email, string p_contrasenia, byte p_fotoPerfil, TipoUsuario p_tipoUsuario)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Usuario_new) ENABLED START*/
        Usuario entity = new Usuario
        {
            NombreUsuario = p_nombreUsuario,
            Email = p_email,
            Contrasenia = _passwordHasher.HashPasswordIfNeeded(p_contrasenia),
            FotoPerfil = p_fotoPerfil,
            EstadoCuenta = EstadoCuenta.Activa,
            TipoUsuario = p_tipoUsuario,
        };

        await _repository.SaveAsync(entity);
        return entity;
        /*PROTECTED REGION END*/
    }
}
