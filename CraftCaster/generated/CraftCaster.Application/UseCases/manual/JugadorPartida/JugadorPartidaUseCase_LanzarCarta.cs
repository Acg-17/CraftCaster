using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_lanzarCarta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

public sealed partial class JugadorPartidaUseCase
{
    public async Task<string> LanzarCartaAsync(int p_oid, int p_carta_oid, int p_objetivoCarta, bool p_objetivoJugador)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_lanzarCarta) ENABLED START*/
        // Comprueba el objetivo, consume el maná, mueve la carta (criatura a la mesa, hechizo al descarte)
        // y ejecuta sus habilidades de entrada con Habilidad.ejecutar. Todo en una transacción: si falla un
        // efecto se deshace también la carta y el maná. p_objetivoCarta = 0 si no hay carta objetivo.
        JugadorPartidaService jugadorPartidaService = null!;
        string result = string.Empty;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.LanzarCartaAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);
            var cartaService = new CartaService(_unitOfWork);
            var habilidadService = new HabilidadService(_unitOfWork);

            // Comprobar existencia de jugador y carta
            var jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid) ??
                throw new ModelException($"No existe el jugador con id {p_oid}.");
            CartaPartida carta = await _unitOfWork.CartaPartidaRepository.ReadAsync(p_carta_oid) ??
                throw new ModelException($"No existe la carta con id {p_carta_oid}.");

            // Comprobar partida en curso, turno activo y rival
            var partida = jugador.Partida ??
                throw new ModelException("El jugador no tiene partida asociada.");
            if (partida.Estado != EstadoPartida.EnCurso)
                throw new ModelException("Solo se pueden lanzar cartas en una partida en curso.");
            if (!jugador.Activo)
                throw new ModelException("El jugador no tiene el turno activo.");
            var rival = partida.Jugadores.FirstOrDefault(j => j.Id != jugador.Id) ??
                throw new ModelException("Falta el rival en la partida.");

            // Comprobar que la carta está en la mano del jugador y que tiene maná suficiente
            var zonaMano = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mano) ??
                throw new ModelException("El jugador no tiene zona de mano.");
            if (carta.Zona == null || carta.Zona.Id != zonaMano.Id)
                throw new ModelException("La carta no está en la zona de mano del jugador.");
            int coste = carta.Original.CosteMana;
            if (coste > jugador.Mana)
                throw new ModelException($"Maná insuficiente: necesitas {coste} y tienes {jugador.Mana}.");

            // Comprobar el objetivo que pide la carta (Carta.modoObjetivo)
            int? objetivoCartaId = p_objetivoCarta > 0 ? p_objetivoCarta : null;
            var campoRival = rival.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)?.Cartas ?? new List<CartaPartida>();
            var campoPropio = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)?.Cartas ?? new List<CartaPartida>();
            int modo = await cartaService.ModoObjetivoAsync(carta.Original.Id, campoRival.Count, campoPropio.Count);
            switch (modo)
            {
                case 1: // enemigo o criatura
                    if (!p_objetivoJugador && objetivoCartaId is null)
                        throw new ModelException($"Debes elegir un objetivo para lanzar {carta.Original.Nombre}.");
                    if (!p_objetivoJugador && !campoRival.Any(c => c.Id == objetivoCartaId))
                        throw new ModelException("La criatura objetivo ya no está en la mesa rival.");
                    break;
                case 2: // criatura enemiga
                    if (objetivoCartaId is null || !campoRival.Any(c => c.Id == objetivoCartaId))
                        throw new ModelException($"Debes elegir una criatura rival en mesa para {carta.Original.Nombre}.");
                    break;
                case 3: // criatura propia
                    if (objetivoCartaId is null || !campoPropio.Any(c => c.Id == objetivoCartaId))
                        throw new ModelException($"Debes elegir una de tus criaturas en mesa para {carta.Original.Nombre}.");
                    break;
            }

            // Consumir el maná y mover la carta
            jugador.Mana -= coste;
            var zonaDestino = carta.Original.Tipo == TipoCarta.Criatura
                ? jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)
                    ?? throw new ModelException("El jugador no tiene zona de campo.")
                : jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Descarte)
                    ?? throw new ModelException("El jugador no tiene zona de descarte.");
            if (carta.Original.Tipo == TipoCarta.Criatura)
            {
                carta.VidaActual = carta.Original.Vida ?? 0;
                carta.AtaqueActual = carta.Original.Ataque ?? 0;
            }
            carta.PuedeAtacar = false;
            zonaMano.Cartas.Remove(carta);
            carta.Zona = zonaDestino;
            zonaDestino.Cartas.Add(carta);

            // Registrar la acción de lanzar carta en el turno actual
            Turno? turnoActual = partida.Turnos.OrderByDescending(t => t.NumTurno).FirstOrDefault();
            if (turnoActual != null && turnoActual.Jugador != null && turnoActual.Jugador.Id == jugador.Id)
            {
                Accion accion = new Accion
                {
                    Name = TipoAccion.Lanzar,
                    Turno = turnoActual,
                    Origen = carta,
                };
                if (objetivoCartaId is int idObjetivo)
                {
                    var cartaObjetivo = await _unitOfWork.CartaPartidaRepository.ReadAsync(idObjetivo);
                    if (cartaObjetivo != null) accion.Objetivo.Add(cartaObjetivo);
                }
                await _unitOfWork.AccionRepository.SaveAsync(accion);
                turnoActual.Acciones.Add(accion);
            }

            await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMano);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDestino);
            await _unitOfWork.JugadorPartidaRepository.SaveAsync(jugador);

            var resumen = new List<string>
            {
                carta.Original.Tipo == TipoCarta.Criatura
                    ? $"Has invocado a {carta.Original.Nombre} en la mesa."
                    : $"Has lanzado el instantáneo {carta.Original.Nombre}.",
            };

            // Habilidades de entrada (todas en un hechizo, las de "Entrar" en una criatura) con Habilidad.ejecutar
            var habilidadesEntrada = carta.Original.Estados
                .Where(e => carta.Original.Tipo == TipoCarta.Hechizo || e.Tipo == TipoEstado.Entrar)
                .SelectMany(e => e.Habilidades)
                .Where(h => h.Tipo != TipoHabilidad.Defensa)
                .ToList();
            foreach (var habilidad in habilidadesEntrada)
            {
                string efecto = await habilidadService.EjecutarAsync(habilidad.Id, carta.Id, p_objetivoCarta, p_objetivoJugador);
                if (!string.IsNullOrWhiteSpace(efecto)) resumen.Add(efecto);
            }

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
            throw new ModelException("Application error while executing use case JugadorPartida.lanzarCarta.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
