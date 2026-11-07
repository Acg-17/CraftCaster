using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Usuario_reportarUsuario) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class UsuarioService
{
    public async Task ReportarUsuarioAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Usuario_reportarUsuario) ENABLED START*/
        Usuario usuario = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe el usuario indicado.");

        usuario.EstadoCuenta = EstadoCuenta.Suspendida;
        await _repository.SaveAsync(usuario);
        /*PROTECTED REGION END*/
    }
}
