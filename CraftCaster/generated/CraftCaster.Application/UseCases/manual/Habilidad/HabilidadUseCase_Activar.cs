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

using Usuario = CraftCaster.Domain.Entities.Usuario;
using Partida = CraftCaster.Domain.Entities.Partida;
using JugadorPartida = CraftCaster.Domain.Entities.JugadorPartida;
using ZonaPartida = CraftCaster.Domain.Entities.ZonaPartida;
using CartaPartida = CraftCaster.Domain.Entities.CartaPartida;
using Carta = CraftCaster.Domain.Entities.Carta;
using Mazo = CraftCaster.Domain.Entities.Mazo;
using Estado = CraftCaster.Domain.Entities.Estado;
using Habilidad = CraftCaster.Domain.Entities.Habilidad;
using Turno = CraftCaster.Domain.Entities.Turno;
using Accion = CraftCaster.Domain.Entities.Accion;


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
            EstadoService estadoService = new EstadoService(_unitOfWork);

            int enemigo = Math.Abs(await partidaService.GetActivoAsync(p_partida_oid)-1);
            int self = await partidaService.GetActivoAsync(p_partida_oid);
            int idEnemigo = await partidaService.GetPlayerAsync(p_partida_oid, enemigo);
            int idSelf = await partidaService.GetPlayerAsync(p_partida_oid, self);

            // Si estas leyendo esto, si, utilizar herencia era una mejor solucion,
            // pero se quedaba el tdm hasta arriba de clases y podria complicarse la cosa.
            // Para la siguiente entrega puede que se refactorize el siguiente bloque.
            switch(habilidad.Tipo)
            {
                case TipoHabilidad.Ataque: //Ataque tiene tipoObjetivo = libre
                    //Si objetivo es 0 se ataca al jugador rival, si es cualquier otro numero a una de sus criaturas
                    
                    if(habilidad.Objetivo == 0)
                    {
                        await jugadorService.ActualizarVidaAsync(idEnemigo, habilidad.Cantidad);
                    } else
                    {
                        int cartaObj = await partidaService.GetCartaAsync(p_partida_oid, idEnemigo, 1, habilidad.Objetivo-1);//Zona 1 es el campo
                        await cartaPartidaService.RecibirDanyoAsync(cartaObj, habilidad.Cantidad);
                    }
                    break;
                case TipoHabilidad.Cura: //Ataque tiene tipoObjetivo = self
                    await jugadorService.ActualizarVidaAsync(idSelf, -habilidad.Cantidad);
                    break;
                case TipoHabilidad.Danyo: //Ataque tiene tipoObjetivo = enemigo/jugador/libre/todosEnemigos
                    switch(habilidad.TipoObjetivo)
                    {
                        case TipoObjetivo.Enemigo:
                            int cartaOb = await partidaService.GetCartaAsync(p_partida_oid, enemigo, 1, habilidad.Objetivo);//Zona 1 es el campo
                            await cartaPartidaService.RecibirDanyoAsync(cartaOb, habilidad.Cantidad);
                            break;
                        case TipoObjetivo.Jugador:
                            await jugadorService.ActualizarVidaAsync(idEnemigo, habilidad.Cantidad);
                            break;
                        case TipoObjetivo.Libre:
                            if(habilidad.Objetivo == 0)
                            {
                                await jugadorService.ActualizarVidaAsync(idEnemigo, habilidad.Cantidad);
                            } else
                            {
                                int cartaObj = await partidaService.GetCartaAsync(p_partida_oid, enemigo, 1, habilidad.Objetivo-1);//Zona 1 es el campo
                                await cartaPartidaService.RecibirDanyoAsync(cartaObj, habilidad.Cantidad);
                            }
                            break;
                        case TipoObjetivo.TodosEnemigos:
                            int id = partida.Jugadores[enemigo].Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)?.Id ?? throw new ModelException("No existe la zona de campo");
                            foreach(int cartaId in await zonaPartidaService.GetAllCartasAsync(id))
                            {
                                await cartaPartidaService.RecibirDanyoAsync(cartaId, habilidad.Cantidad);
                            }
                            break;
                        default:
                            throw new ModelException($"El tipo de objetivo de la habilidad con id {p_oid} no es valido o no ha sido implementado");
                    }
                    break;
                case TipoHabilidad.Descartar:
                    ZonaPartida mano = partida.Jugadores[enemigo].Zonas[0] ?? throw new ModelException("No existe la zona de destino");
                    ZonaPartida descarte = partida.Jugadores[enemigo].Zonas[2] ?? throw new ModelException("No existe la zona de destino");
                    CartaPartida carta = mano.Cartas.Count() < habilidad.Objetivo ?  mano.Cartas[habilidad.Objetivo] : throw new ModelException($"El objetivo está fuera de rango");
                    carta.Zona = await _unitOfWork.ZonaPartidaRepository.ReadAsync(descarte.Id) ?? throw new ModelException("No existe la zona de destino");
                    mano.Cartas.Remove(carta);
                    descarte.Cartas.Add(carta);
                    break;
                case TipoHabilidad.Destruir:
                    ZonaPartida campo = partida.Jugadores[enemigo].Zonas[1] ?? throw new ModelException("No existe la zona de destino");;
                    ZonaPartida pilaDescarte = partida.Jugadores[enemigo].Zonas[2] ?? throw new ModelException("No existe la zona de destino");;
                    CartaPartida criatura = campo.Cartas.Count() < habilidad.Objetivo ?  campo.Cartas[habilidad.Objetivo] : throw new ModelException($"El objetivo está fuera de rango");
                    Estado? estado = criatura.Original.Estados.FirstOrDefault(x=> x.Tipo == TipoEstado.Morir);
                    if (estado != null)
                        await estadoService.ActivarAsync(estado.Id);
                    criatura.Zona = await _unitOfWork.ZonaPartidaRepository.ReadAsync(pilaDescarte.Id) ?? throw new ModelException("No existe la zona de destino");
                    campo.Cartas.Remove(criatura);
                    pilaDescarte.Cartas.Add(criatura);
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
