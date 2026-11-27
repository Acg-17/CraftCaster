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
                        // GetCarta espera el indice del jugador (0/1), no su Id, y la zona como TipoZona (Campo = 2)
                        int cartaObj = await partidaService.GetCartaAsync(p_partida_oid, enemigo, (int)TipoZona.Campo, habilidad.Objetivo-1);
                        await cartaPartidaService.RecibirDanyoAsync(cartaObj, habilidad.Cantidad);
                    }
                    break;
                case TipoHabilidad.Cura: // Suma vida a la criatura propia (si esta en campo y objetivo Self) o al jugador activo
                    if (habilidad.TipoObjetivo == TipoObjetivo.Self)
                    {
                        Domain.Entities.ZonaPartida? campoPropio = partida.Jugadores[self].Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
                        CartaPartida? criaturaPropia = campoPropio is not null && campoPropio.Cartas.Count > 0
                            ? (habilidad.Objetivo >= 0 && habilidad.Objetivo < campoPropio.Cartas.Count
                                ? campoPropio.Cartas[habilidad.Objetivo]
                                : campoPropio.Cartas.FirstOrDefault(c => c.Original?.Id == habilidad.Estado?.Carta?.Id))
                            : null;
                        if (criaturaPropia is not null)
                        {
                            criaturaPropia.VidaActual += habilidad.Cantidad;
                            await _unitOfWork.CartaPartidaRepository.SaveAsync(criaturaPropia);
                        }
                        else
                        {
                            await jugadorService.ActualizarVidaAsync(idSelf, -habilidad.Cantidad);
                        }
                    }
                    else
                    {
                        await jugadorService.ActualizarVidaAsync(idSelf, -habilidad.Cantidad);
                    }
                    break;
                case TipoHabilidad.Defensa: // Daño al defender: devuelve daño al atacante (criatura rival o jugador rival si fue hechizo)
                    if (habilidad.Objetivo == 0)
                    {
                        await jugadorService.ActualizarVidaAsync(idEnemigo, habilidad.Cantidad);
                    }
                    else
                    {
                        int cartaAtacante = await partidaService.GetCartaAsync(p_partida_oid, enemigo, (int)TipoZona.Campo, habilidad.Objetivo - 1);
                        await cartaPartidaService.RecibirDanyoAsync(cartaAtacante, habilidad.Cantidad);
                    }
                    break;
                case TipoHabilidad.Danyo: //Ataque tiene tipoObjetivo = enemigo/jugador/libre/todosEnemigos
                    switch(habilidad.TipoObjetivo)
                    {
                        case TipoObjetivo.Enemigo:
                            int cartaOb = await partidaService.GetCartaAsync(p_partida_oid, enemigo, (int)TipoZona.Campo, habilidad.Objetivo);
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
                                int cartaObj = await partidaService.GetCartaAsync(p_partida_oid, enemigo, (int)TipoZona.Campo, habilidad.Objetivo-1);
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
                    Domain.Entities.ZonaPartida mano = partida.Jugadores[enemigo].Zonas.FirstOrDefault(x=> x.TipoZona == TipoZona.Mano) ?? throw new ModelException("No existe la zona de destino");
                    Domain.Entities.ZonaPartida descarte = partida.Jugadores[enemigo].Zonas.FirstOrDefault(x=> x.TipoZona == TipoZona.Descarte) ?? throw new ModelException("No existe la zona de destino");
                    CartaPartida carta = mano.Cartas.Count() > habilidad.Objetivo ?  mano.Cartas[habilidad.Objetivo] : throw new ModelException($"El objetivo está fuera de rango");
                    carta.Zona = await _unitOfWork.ZonaPartidaRepository.ReadAsync(descarte.Id) ?? throw new ModelException("No existe la zona de destino");
                    mano.Cartas.Remove(carta);
                    descarte.Cartas.Add(carta);
                    break;
                case TipoHabilidad.Destruir:
                    int jugadorObjetivoIdx = habilidad.TipoObjetivo == TipoObjetivo.Jugador ? self : enemigo;
                    Domain.Entities.ZonaPartida campo = partida.Jugadores[jugadorObjetivoIdx].Zonas.FirstOrDefault(x=> x.TipoZona == TipoZona.Campo) ?? throw new ModelException("No existe la zona de destino");
                    Domain.Entities.ZonaPartida pilaDescarte = partida.Jugadores[jugadorObjetivoIdx].Zonas.FirstOrDefault(x=> x.TipoZona == TipoZona.Descarte) ?? throw new ModelException("No existe la zona de destino");
                    CartaPartida criatura = campo.Cartas.Count() > habilidad.Objetivo ?  campo.Cartas[habilidad.Objetivo] : throw new ModelException($"El objetivo está fuera de rango");
                    Estado? estado = criatura.Original.Estados.FirstOrDefault(x=> x.Tipo == TipoEstado.Morir);
                    if (estado != null)
                        await estadoService.ActivarAsync(estado.Id);
                    criatura.Zona = await _unitOfWork.ZonaPartidaRepository.ReadAsync(pilaDescarte.Id) ?? throw new ModelException("No existe la zona de destino");
                    campo.Cartas.Remove(criatura);
                    pilaDescarte.Cartas.Add(criatura);
                    break;
                case TipoHabilidad.Robar:
                    for(int i = 0; i < habilidad.Cantidad; i++)
                        await jugadorService.RobarCartaDelMazoAsync(idSelf); // Id del jugador, no su indice
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
