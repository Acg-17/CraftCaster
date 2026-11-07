using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_JugadorPartida_toggleActivo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class JugadorPartidaService
{
    public async Task ToggleActivoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_JugadorPartida_toggleActivo) ENABLED START*/
        JugadorPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe el JugadorPartida indicado.");

        entity.Activo = !entity.Activo;
        await _repository.SaveAsync(entity);
        /*PROTECTED REGION END*/
    }
}
