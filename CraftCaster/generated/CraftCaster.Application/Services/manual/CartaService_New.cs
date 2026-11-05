using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Carta_new) ENABLED START*/
// references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class CartaService
{
    public async Task<Carta> NewAsync(string p_nombre, Rareza p_rareza, int p_costeMana, TipoCarta p_tipo, string p_descripcion, byte p_imagen, bool p_esPublica, int p_ataque, int p_vida, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Carta_new) ENABLED START*/
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Carta.Usuario.");

        Carta entity = new Carta
        {
            Nombre = p_nombre,
            Rareza = p_rareza,
            CosteMana = p_costeMana,
            Tipo = p_tipo,
            Descripcion = p_descripcion,
            Imagen = p_imagen,
            EsPublica = p_esPublica,
            Usuario = usuarioEntity,
        };

        if (entity.Tipo == TipoCarta.Criatura)
        {
            entity.Ataque = p_ataque;
            entity.Vida = p_vida;
        }

        await _repository.SaveAsync(entity);
        return entity;
        /*PROTECTED REGION END*/
    }
}
