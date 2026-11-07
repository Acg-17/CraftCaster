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
        UsuarioService usuarioService = null!;
        global::CraftCaster.Domain.Entities.Usuario result = default(global::CraftCaster.Domain.Entities.Usuario)!;

        try
        {
            if (string.IsNullOrWhiteSpace(p_nombreUsuario))
            {
                throw new ModelException("El nombre de usuario no puede estar vacio.");
            }

            if (string.IsNullOrWhiteSpace(p_email))
            {
                throw new ModelException("El email no puede estar vacio.");
            }

            if (string.IsNullOrWhiteSpace(p_contrasenia))
            {
                throw new ModelException("La contrasenia no puede estar vacia.");
            }

            await _unitOfWork.BeginTransactionAsync("UsuarioUseCase.RegistrarUsuarioAsync");
            usuarioService = new UsuarioService(_unitOfWork);

            var usuarios = await _unitOfWork.UsuarioRepository.ReadAllAsync();
            if (usuarios.Any(u => u.Email.Equals(p_email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ModelException("Ya existe un usuario con el email indicado.");
            }

            if (usuarios.Any(u => u.NombreUsuario.Equals(p_nombreUsuario, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ModelException("Ya existe un usuario con el nombre indicado.");
            }

            result = await usuarioService.NewAsync(p_nombreUsuario, p_email, p_contrasenia, p_fotoPerfil, TipoUsuario.Cliente);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case Usuario.registrarUsuario.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
