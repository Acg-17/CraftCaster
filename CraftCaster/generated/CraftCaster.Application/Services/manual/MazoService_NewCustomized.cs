using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Mazo_newCustomized) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class MazoService
{
    public async Task<Mazo> NewCustomizedAsync(string p_nombre, bool p_esPublico, byte p_imagen, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Mazo_newCustomized) ENABLED START*/
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Mazo.Usuario.");

        Mazo entity = new Mazo
        {
            Nombre = p_nombre,
            EsPublico = p_esPublico,
            Imagen = p_imagen,
            Usuario = usuarioEntity,
            NumCartas = 0,
            CosteMedio = 0,
        };

        await _repository.SaveAsync(entity);
        return entity;
        /*PROTECTED REGION END*/
    }
}
