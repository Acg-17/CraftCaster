using System;
using System.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Usuario_unirsePartida) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Usuario;

public sealed partial class UsuarioUseCase
{
    public async Task UnirsePartidaAsync(int p_oid, int p_partida_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Usuario_unirsePartida) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        UsuarioService usuarioService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("UsuarioUseCase.UnirsePartidaAsync");
            usuarioService = new UsuarioService(_unitOfWork);

            // 1. Buscamos al usuario que quiere unirse como visitante
            Usuario usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el usuario indicado.");

            // 2. Buscamos la partida a la que se quiere unir
            Partida partida = await _unitOfWork.PartidaRepository.ReadAsync(p_partida_oid)
                ?? throw new ModelException("No existe la partida indicada.");

            if (partida.Estado != EstadoPartida.Espera)
            {
                throw new ModelException("La partida no se encuentra en estado de espera.");
            }

            if (partida.Usuario2 != null)
            {
                throw new ModelException("La partida ya cuenta con dos jugadores.");
            }

            if (partida.Usuario1.Id == p_oid)
            {
                throw new ModelException("No puedes unirte como visitante a una partida creada por ti.");
            }

            // 3. Buscamos el mazo del visitante
            Mazo? mazo = usuario.Mazos.FirstOrDefault(m => m.EstadoMazo == EstadoMazo.Jugable)
                ?? usuario.Mazos.FirstOrDefault()
                ?? throw new ModelException("El usuario no tiene ningún mazo para unirse a la partida.");

            // 4. Asignamos al usuario 2 y su mazo a la partida
            partida.Usuario2 = usuario;
            partida.Mazo2 = mazo;
            partida.Estado = EstadoPartida.EnCurso;

            usuario.PartidasVisitante.Add(partida);

            // 5. Creamos al Jugador 2 en la partida
            JugadorPartida jugador2 = new JugadorPartida
            {
                NumJugador = 2,
                Vida = 30,
                Mana = 0,
                Activo = false,
                Partida = partida
            };

            await _unitOfWork.JugadorPartidaRepository.SaveAsync(jugador2);
            partida.Jugadores.Add(jugador2);

            // 6. Creamos las 4 zonas del tablero para el Jugador 2
            ZonaPartida zonaMazo = new ZonaPartida { TipoZona = TipoZona.Mazo, Jugador = jugador2 };
            ZonaPartida zonaMano = new ZonaPartida { TipoZona = TipoZona.Mano, Jugador = jugador2 };
            ZonaPartida zonaCampo = new ZonaPartida { TipoZona = TipoZona.Campo, Jugador = jugador2 };
            ZonaPartida zonaDescarte = new ZonaPartida { TipoZona = TipoZona.Descarte, Jugador = jugador2 };

            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMazo);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMano);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaCampo);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDescarte);

            jugador2.Zonas.Add(zonaMazo);
            jugador2.Zonas.Add(zonaMano);
            jugador2.Zonas.Add(zonaCampo);
            jugador2.Zonas.Add(zonaDescarte);

            // 7. Instanciamos las cartas del mazo 2 en su zona Mazo
            foreach (var linea in mazo.Lineas)
            {
                for (int i = 0; i < linea.Cantidad; i++)
                {
                    CartaPartida cartaPartida = new CartaPartida
                    {
                        Original = linea.Carta,
                        Zona = zonaMazo,
                        VidaActual = linea.Carta.Vida ?? 0,
                        AtaqueActual = linea.Carta.Ataque ?? 0,
                        PuedeAtacar = false
                    };

                    await _unitOfWork.CartaPartidaRepository.SaveAsync(cartaPartida);
                    zonaMazo.Cartas.Add(cartaPartida);
                }
            }

            await _unitOfWork.PartidaRepository.SaveAsync(partida);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case Usuario.unirsePartida.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
