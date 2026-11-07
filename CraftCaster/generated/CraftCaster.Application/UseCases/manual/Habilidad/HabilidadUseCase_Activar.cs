using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Habilidad_activar) ENABLED START*/
        // references to other libraries
        using System.Collections;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Habilidad;

public sealed partial class HabilidadUseCase
{
    public async Task ActivarAsync(int p_oid, int p_partida_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Habilidad_activar) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        HabilidadService habilidadService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("HabilidadUseCase.ActivarAsync");
            habilidadService = new HabilidadService(_unitOfWork);

            // Write here your custom transaction code...
            // Use habilidadService, _unitOfWork.HabilidadRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.
            Domain.Entities.Habilidad habilidad = await _unitOfWork.HabilidadRepository.ReadAsync(p_oid) ?? throw new ModelException("No existe la habilidad indicada.");
            Domain.Entities.Partida partida = await _unitOfWork.PartidaRepository.ReadAsync(p_partida_oid) ?? throw new ModelException("No existe la partida indicada.");
            PartidaService partidaService = new PartidaService(_unitOfWork);
            JugadorPartidaService jugadorService = new JugadorPartidaService(_unitOfWork);
            CartaPartidaService cartaPartidaService = new CartaPartidaService(_unitOfWork);
            ZonaPartidaService zonaPartidaService = new ZonaPartidaService(_unitOfWork);

            int enemigo = Math.Abs(await partidaService.GetActivoAsync(p_partida_oid)-1);
            int self = await partidaService.GetActivoAsync(p_partida_oid);

            // Si estas leyendo esto, si, utilizar herencia era una mejor solucion,
            // pero se quedaba el tdm hasta arriba de clases y podria complicarse la cosa.
            // Para la siguiente entrega puede que se refactorize el siguiente bloque.
            switch(habilidad.Tipo)
            {
                case TipoHabilidad.Ataque: //Ataque tiene tipoObjetivo = libre
                    //Si objetivo es 0 se ataca al jugador rival, si es cualquier otro numero a una de sus criaturas
                    if(habilidad.Objetivo == 0)
                    {
                        await jugadorService.ActualizarVidaAsync(await partidaService.GetPlayerAsync(p_partida_oid, enemigo), habilidad.Cantidad);
                    } else
                    {
                        int carta = await partidaService.GetCartaAsync(p_partida_oid, enemigo, 1, habilidad.Objetivo-1);//Zona 1 es el campo
                        await cartaPartidaService.RecibirDanyoAsync(carta, habilidad.Cantidad);
                    }
                    break;
                case TipoHabilidad.Cura: //Ataque tiene tipoObjetivo = self
                    await jugadorService.ActualizarVidaAsync(self, -habilidad.Cantidad);
                    break;
                case TipoHabilidad.Danyo: //Ataque tiene tipoObjetivo = enemigo/jugador/libre/todosEnemigos
                    switch(habilidad.TipoObjetivo)
                    {
                        case TipoObjetivo.Enemigo:
                            int carta = await partidaService.GetCartaAsync(p_partida_oid, enemigo, 1, habilidad.Objetivo);//Zona 1 es el campo
                            await cartaPartidaService.RecibirDanyoAsync(carta, habilidad.Cantidad);
                            break;
                        case TipoObjetivo.Jugador:
                            await jugadorService.ActualizarVidaAsync(await partidaService.GetPlayerAsync(p_partida_oid, enemigo), habilidad.Cantidad);
                            break;
                        case TipoObjetivo.Libre:
                            if(habilidad.Objetivo == 0)
                            {
                                await jugadorService.ActualizarVidaAsync(await partidaService.GetPlayerAsync(p_partida_oid, enemigo), habilidad.Cantidad);
                            } else
                            {
                                int cartaObj = await partidaService.GetCartaAsync(p_partida_oid, enemigo, 1, habilidad.Objetivo-1);//Zona 1 es el campo
                                await cartaPartidaService.RecibirDanyoAsync(cartaObj, habilidad.Cantidad);
                            }
                            break;
                        case TipoObjetivo.TodosEnemigos:
                            int id = partida.Jugadores[enemigo].Zonas[1].Id;
                            foreach(int cartaId in await zonaPartidaService.GetAllCartasAsync(id))
                            {
                                int cartaObj = await partidaService.GetCartaAsync(p_partida_oid, enemigo, 1, cartaId);//Zona 1 es el campo
                                await cartaPartidaService.RecibirDanyoAsync(cartaObj, habilidad.Cantidad);
                            }
                            break;
                        default:
                            throw new ModelException($"El tipo de objetivo de la habilidad con id {p_oid} no es valido o no ha sido implementado");
                    }
                    break;
                case TipoHabilidad.Defensa:
                    break;
                case TipoHabilidad.Descartar:
                    break;
                case TipoHabilidad.Destruir:
                    break;
                case TipoHabilidad.Robar:
                    for(int i = 0; i < habilidad.Cantidad; i++)
                        await jugadorService.RobarCartaAsync(self);
                    break;
                default:
                    throw new ModelException($"El tipo de la habilidad con id {p_oid} no es valido o no ha sido implementado");
            }
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Habilidad.activar.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
