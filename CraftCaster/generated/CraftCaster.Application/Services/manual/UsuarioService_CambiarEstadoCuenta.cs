using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Usuario_cambiarEstadoCuenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class UsuarioService
{
    public async Task CambiarEstadoCuentaAsync(int p_oid, EstadoCuenta p_estadoCuenta)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Usuario_cambiarEstadoCuenta) ENABLED START*/
        Usuario? usuario = await _repository.ReadAsync(p_oid);
        if (usuario == null)
        {
            throw new ModelException("No existe el usuario indicado.");
        }

        usuario.EstadoCuenta = p_estadoCuenta;
        await _repository.SaveAsync(usuario);
        /*PROTECTED REGION END*/
    }
}
