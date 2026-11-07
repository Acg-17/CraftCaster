using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_lanzarCarta) ENABLED START*/
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
    public async Task LanzarCartaAsync(int p_oid, int p_carta_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_lanzarCarta) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.LanzarCartaAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            // Comprobar existencia de jugador
            var jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid) ??
                throw new ModelException($"No existe el jugador con id {p_oid}.");

            // Comprobar existencia de carta
            CartaPartida carta = await _unitOfWork.CartaPartidaRepository.ReadAsync(p_carta_oid) ??
                throw new ModelException($"No existe la carta con id {p_carta_oid}.");

            // Comprobar partida en curso
            var partida = jugador.Partida ??
                throw new ModelException($"El jugador no tiene partida asociada.");
            if (partida.Estado != EstadoPartida.EnCurso)
                throw new ModelException($"Solo se pueden lanzar cartas en una partida en curso.");

            // Comprobar turno activo
            if (!jugador.Activo)
                throw new ModelException($"El jugador no tiene el turno activo.");

            // Comprobar que la carta está en la zona de mano del jugador
            var zonaMano = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mano) ??
                throw new ModelException($"El jugador no tiene zona de mano.");
            if (carta.Zona == null || carta.Zona.Id != zonaMano.Id)
                throw new ModelException($"La carta no está en la zona de mano del jugador.");
            
            // Comprobar que el jugador tiene suficiente mana para lanzar la carta
            int coste = carta.Original.CosteMana;
            if (coste > jugador.Mana)
                throw new ModelException($"Mana insuficiente: coste: {coste} y mana disponible: {jugador.Mana}.");

            // Restar el mana del jugador
            jugador.Mana -= coste;

            // 
            var zonaDestino = carta.Original.Tipo == TipoCarta.Criatura
                ? jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)
                    ?? throw new ModelException($"El jugador no tiene zona de campo.")
                : jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Descarte)
                    ?? throw new ModelException($"El jugador no tiene zona de descarte.");
            
            // Actualizar la carta
            if (carta.Original.Tipo == TipoCarta.Criatura)
            {
                carta.VidaActual = carta.Original.Vida ?? 0;
                carta.AtaqueActual = carta.Original.Ataque ?? 0;
            }
            carta.PuedeAtacar = false;

            // Mover la cartade mano a la zona
            zonaMano.Cartas.Remove(carta);
            carta.Zona = zonaDestino;
            zonaDestino.Cartas.Add(carta);

            Turno? turnoActual = partida.Turnos.OrderByDescending(t => t.NumTurno).FirstOrDefault();

            // Registrar la acción de lanzar carta en el turno actual
            if (turnoActual != null && turnoActual.Jugador != null && turnoActual.Jugador.Id == jugador.Id)
            {
                Accion accion = new Accion
                {
                    Name = TipoAccion.Lanzar,
                    Turno = turnoActual,
                    Origen = carta,
                };
                await _unitOfWork.AccionRepository.SaveAsync(accion);
                turnoActual.Acciones.Add(accion);
            }

            // Guardar cambios
            await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMano);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDestino);
            await _unitOfWork.JugadorPartidaRepository.SaveAsync(jugador);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case JugadorPartida.lanzarCarta.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
