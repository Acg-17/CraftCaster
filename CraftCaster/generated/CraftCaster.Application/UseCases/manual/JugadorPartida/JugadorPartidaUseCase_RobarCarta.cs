using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_robarCarta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

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


public sealed partial class JugadorPartidaUseCase
{
    public async Task RobarCartaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_robarCarta) ENABLED START*/
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.RobarCartaAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            JugadorPartida jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el jugador indicado.");

            ZonaPartida zonaMazo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo)
                ?? throw new ModelException("El jugador no dispone de zona de mazo.");

            ZonaPartida zonaMano = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mano)
                ?? throw new ModelException("El jugador no dispone de zona de mano.");

            if (zonaMazo.Cartas == null || zonaMazo.Cartas.Count == 0)
            {
                throw new ModelException("El mazo esta vacio, no quedan cartas para robar.");
            }

            CartaPartida carta = zonaMazo.Cartas[0];
            zonaMazo.Cartas.Remove(carta);

            // Si la mano tiene menos de 10 cartas, se añade a la mano; si no, se quema al descarte
            if (zonaMano.Cartas != null && zonaMano.Cartas.Count >= 10)
            {
                ZonaPartida? zonaDescarte = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Descarte);
                if (zonaDescarte != null)
                {
                    carta.Zona = zonaDescarte;
                    zonaDescarte.Cartas.Add(carta);
                    await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDescarte);
                }
            }
            else
            {
                carta.Zona = zonaMano;
                zonaMano.Cartas?.Add(carta);
                await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMano);
            }

            await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMazo);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case JugadorPartida.robarCarta.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
