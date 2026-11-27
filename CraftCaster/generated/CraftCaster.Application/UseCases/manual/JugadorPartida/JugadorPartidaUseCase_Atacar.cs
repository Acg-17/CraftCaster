using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_atacar) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

public sealed partial class JugadorPartidaUseCase
{
    public async Task<string> AtacarAsync(int p_oid, int p_atacante_oid, int p_objetivoCarta, bool p_objetivoJugador)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_atacar) ENABLED START*/
        // Ataque de una criatura propia a una criatura rival (p_objetivoCarta) o, solo si la mesa rival está
        // vacía, al jugador rival. Solo hace daño la atacante, salvo que la defensora tenga la habilidad Defensa
        // ("daño al defender"). Las habilidades "Al atacar" y "Al morir" se ejecutan con Habilidad.ejecutar.
        // Todo en una transacción.
        JugadorPartidaService jugadorPartidaService = null!;
        string result = string.Empty;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.AtacarAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);
            var cartaPartidaService = new CartaPartidaService(_unitOfWork);
            var habilidadService = new HabilidadService(_unitOfWork);

            var jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid) ??
                throw new ModelException($"No existe el jugador con id {p_oid}.");
            var partida = jugador.Partida ??
                throw new ModelException("El jugador no tiene partida asociada.");
            if (partida.Estado != EstadoPartida.EnCurso)
                throw new ModelException("La partida no está en curso.");
            if (!jugador.Activo)
                throw new ModelException("No es tu turno.");
            var rival = partida.Jugadores.FirstOrDefault(j => j.Id != jugador.Id) ??
                throw new ModelException("Falta el rival en la partida.");

            var campoPropio = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo) ??
                throw new ModelException("No dispones de zona de mesa.");
            var campoRival = rival.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo) ??
                throw new ModelException("El rival no dispone de zona de mesa.");

            // Comprobar la criatura atacante
            CartaPartida atacante = campoPropio.Cartas.FirstOrDefault(c => c.Id == p_atacante_oid) ??
                throw new ModelException("La criatura atacante no está en tu mesa.");
            if (atacante.Original.Tipo != TipoCarta.Criatura)
                throw new ModelException("Solo las criaturas pueden atacar desde la mesa.");
            if (!atacante.PuedeAtacar)
                throw new ModelException($"{atacante.Original.Nombre} no puede atacar en este turno.");
            if (atacante.AtaqueActual <= 0)
                throw new ModelException($"{atacante.Original.Nombre} tiene 0 de ataque y no puede atacar.");

            // Comprobar el objetivo: con criaturas rivales en la mesa hay que atacar a una de ellas
            CartaPartida? defensor = null;
            bool atacaAlJugador = false;
            if (campoRival.Cartas.Count > 0)
            {
                if (p_objetivoJugador || p_objetivoCarta <= 0)
                    throw new ModelException("No puedes atacar al jugador rival mientras aún tenga criaturas en la mesa.");
                defensor = campoRival.Cartas.FirstOrDefault(c => c.Id == p_objetivoCarta) ??
                    throw new ModelException("La carta objetivo no está en la mesa del rival.");
            }
            else
            {
                if (p_objetivoCarta > 0 && !p_objetivoJugador)
                    throw new ModelException("No hay criaturas rivales en mesa; apunta al jugador enemigo.");
                atacaAlJugador = true;
            }

            // La atacante ya ha atacado este turno
            atacante.PuedeAtacar = false;
            await _unitOfWork.CartaPartidaRepository.SaveAsync(atacante);

            // Registrar la acción de ataque en el turno actual
            Turno? turnoActual = partida.Turnos.OrderByDescending(t => t.NumTurno).FirstOrDefault();
            if (turnoActual != null && turnoActual.Jugador != null && turnoActual.Jugador.Id == jugador.Id)
            {
                Accion accion = new Accion
                {
                    Name = TipoAccion.Ataque,
                    Turno = turnoActual,
                    Origen = atacante,
                };
                if (defensor != null) accion.Objetivo.Add(defensor);
                await _unitOfWork.AccionRepository.SaveAsync(accion);
                turnoActual.Acciones.Add(accion);
            }

            var resumen = new List<string>();
            var efectos = new List<string>();

            // Habilidades (sin Defensa) de un momento concreto de una carta
            List<Domain.Entities.Habilidad> HabilidadesDe(CartaPartida carta, TipoEstado momento) =>
                carta.Original.Estados
                    .Where(e => e.Tipo == momento)
                    .SelectMany(e => e.Habilidades)
                    .Where(h => h.Tipo != TipoHabilidad.Defensa)
                    .ToList();

            // Daño a una criatura con CartaPartida.recibirDanyo y, si muere, sus habilidades "Al morir"
            async Task<bool> DanyarCriaturaAsync(CartaPartida criatura, int danyo)
            {
                if (danyo <= 0 || criatura.Zona?.TipoZona != TipoZona.Campo || criatura.VidaActual <= 0) return false;
                await cartaPartidaService.RecibirDanyoAsync(criatura.Id, danyo);
                if (criatura.VidaActual > 0) return false;
                foreach (var habilidadMorir in HabilidadesDe(criatura, TipoEstado.Morir))
                {
                    string efecto = await habilidadService.EjecutarAsync(habilidadMorir.Id, criatura.Id, 0, false);
                    if (!string.IsNullOrWhiteSpace(efecto)) efectos.Add(efecto);
                }
                return true;
            }

            // 1. Habilidades "Al atacar" de la atacante
            foreach (var habilidad in HabilidadesDe(atacante, TipoEstado.Atacar))
            {
                string efecto = await habilidadService.EjecutarAsync(habilidad.Id, atacante.Id, defensor?.Id ?? 0, atacaAlJugador);
                if (!string.IsNullOrWhiteSpace(efecto)) efectos.Add(efecto);
            }

            // 2. Ataque principal
            if (atacaAlJugador)
            {
                await jugadorPartidaService.ActualizarVidaAsync(rival.Id, atacante.AtaqueActual);
                resumen.Add($"{atacante.Original.Nombre} ataca al rival por {atacante.AtaqueActual} de daño.");
            }
            else if (defensor != null && defensor.Zona?.TipoZona == TipoZona.Campo && defensor.VidaActual > 0)
            {
                // Solo si el defensor sigue en la mesa tras las habilidades "Al atacar"
                int danyoDefensa = defensor.Original.Estados
                    .SelectMany(e => e.Habilidades)
                    .Where(h => h.Tipo == TipoHabilidad.Defensa)
                    .Sum(h => Math.Max(0, h.Cantidad));
                string nombreDefensor = defensor.Original.Nombre;

                bool murioDefensor = await DanyarCriaturaAsync(defensor, atacante.AtaqueActual);
                resumen.Add(murioDefensor
                    ? $"{atacante.Original.Nombre} inflige {atacante.AtaqueActual} de daño a {nombreDefensor} y la destruye."
                    : $"{atacante.Original.Nombre} inflige {atacante.AtaqueActual} de daño a {nombreDefensor} (queda con {defensor.VidaActual} de vida).");

                if (danyoDefensa > 0 && atacante.Zona?.TipoZona == TipoZona.Campo && atacante.VidaActual > 0)
                {
                    bool murioAtacante = await DanyarCriaturaAsync(atacante, danyoDefensa);
                    resumen.Add(murioAtacante
                        ? $"{nombreDefensor} devuelve {danyoDefensa} de daño al defender y destruye a {atacante.Original.Nombre}."
                        : $"{nombreDefensor} devuelve {danyoDefensa} de daño al defender a {atacante.Original.Nombre}.");
                }
            }

            resumen.AddRange(efectos);
            result = string.Join(" ", resumen);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case JugadorPartida.atacar.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
