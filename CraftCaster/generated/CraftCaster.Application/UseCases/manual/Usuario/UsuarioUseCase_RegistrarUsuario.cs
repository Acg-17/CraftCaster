using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Usuario_registrarUsuario) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Usuario;

public sealed partial class UsuarioUseCase
{
    public async Task<global::CraftCaster.Domain.Entities.Usuario> RegistrarUsuarioAsync(string p_nombreUsuario, string p_email, string p_contrasenia, byte p_fotoPerfil)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Usuario_registrarUsuario) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        UsuarioService usuarioService = null!;
        global::CraftCaster.Domain.Entities.Usuario result = default(global::CraftCaster.Domain.Entities.Usuario)!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("UsuarioUseCase.RegistrarUsuarioAsync");
            usuarioService = new UsuarioService(_unitOfWork);

            // Write here your custom transaction code...
            // Use usuarioService, _unitOfWork.UsuarioRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Usuario.registrarUsuario.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
