using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_inicioTurno) ENABLED START*/
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
    public async Task InicioTurnoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_inicioTurno) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.InicioTurnoAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            JugadorPartida jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el jugador indicado.");

            // 1. Establece activo en true
            jugador.Activo = true;

            // 2. Regenera y aumenta el mana (maximo 10)
            int manaActualizado = Math.Min(10, Math.Max(1, jugador.Mana + 1));
            jugador.Mana = manaActualizado;

            // 3. Reactiva el ataque para todas las criaturas en su zona de Campo
            ZonaPartida? zonaCampo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
            if (zonaCampo?.Cartas != null)
            {
                foreach (var carta in zonaCampo.Cartas)
                {
                    carta.PuedeAtacar = true;
                    await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
                }
            }

            // 4. Roba una carta del mazo a la mano al iniciar el turno
            ZonaPartida? zonaMazo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
            ZonaPartida? zonaMano = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mano);

            if (zonaMazo?.Cartas != null && zonaMazo.Cartas.Count > 0 && zonaMano != null)
            {
                CartaPartida cartaRobada = zonaMazo.Cartas[0];
                zonaMazo.Cartas.Remove(cartaRobada);

                if (zonaMano.Cartas != null && zonaMano.Cartas.Count < 10)
                {
                    cartaRobada.Zona = zonaMano;
                    zonaMano.Cartas.Add(cartaRobada);
                    await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMano);
                }
                else
                {
                    ZonaPartida? zonaDescarte = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Descarte);
                    if (zonaDescarte != null)
                    {
                        cartaRobada.Zona = zonaDescarte;
                        zonaDescarte.Cartas.Add(cartaRobada);
                        await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDescarte);
                    }
                }

                await _unitOfWork.CartaPartidaRepository.SaveAsync(cartaRobada);
                await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMazo);
            }

            await _unitOfWork.JugadorPartidaRepository.SaveAsync(jugador);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case JugadorPartida.inicioTurno.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
