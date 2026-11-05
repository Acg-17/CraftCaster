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
        // Write here your custom code...

        throw new ModelException("Application service generation for Usuario.cambiarEstadoCuenta is pending.", new NotImplementedException("Method CambiarEstadoCuentaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
