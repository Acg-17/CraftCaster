using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Usuario_iniciarPartida) ENABLED START*/
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Usuario;

public sealed partial class UsuarioUseCase
{
    public async Task IniciarPartidaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Usuario_iniciarPartida) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        UsuarioService usuarioService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("UsuarioUseCase.IniciarPartidaAsync");
            usuarioService = new UsuarioService(_unitOfWork);

            // Custom code:
            // Buscamos usuario que quiere crear la partida
            Domain.Entities.Usuario usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_oid) 
                ?? throw new ModelException("No existe el usuario indicado");

            // Buscamos mazo
            Mazo? mazo = usuario.Mazos.FirstOrDefault(m => m.EstadoMazo == EstadoMazo.Jugable) 
                ?? usuario.Mazos.FirstOrDefault() 
                ?? throw new ModelException("El usuario no tiene ningún mazo para iniciar la partida.");

            // Creamos la partida
            Domain.Entities.Partida partida = new Domain.Entities.Partida
            {
                Estado = EstadoPartida.Espera,
                FechaInicio = DateTime.UtcNow,
                Usuario1 = usuario,
                Mazo1 = mazo
            };

            await _unitOfWork.PartidaRepository.SaveAsync(partida);
            usuario.PartidasLocal.Add(partida);

            // Creamos al jugador 1 en la partida con 30 de vida
            Domain.Entities.JugadorPartida jugador = new Domain.Entities.JugadorPartida
            {
                NumJugador = 1,
                Vida = 30,
                Mana = 0,
                Activo = false,
                Partida = partida
            };

            await _unitOfWork.JugadorPartidaRepository.SaveAsync(jugador);
            partida.Jugadores.Add(jugador);

            // Creamos las 4 zonas del tablero para este jugador
            Domain.Entities.ZonaPartida zonaMazo = new Domain.Entities.ZonaPartida { TipoZona = TipoZona.Mazo, Jugador = jugador };
            Domain.Entities.ZonaPartida zonaMano = new Domain.Entities.ZonaPartida { TipoZona = TipoZona.Mano, Jugador = jugador };
            Domain.Entities.ZonaPartida zonaCampo = new Domain.Entities.ZonaPartida { TipoZona = TipoZona.Campo, Jugador = jugador };
            Domain.Entities.ZonaPartida zonaDescarte = new Domain.Entities.ZonaPartida { TipoZona = TipoZona.Descarte, Jugador = jugador };
           
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMazo);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMano);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaCampo);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDescarte);
            
            jugador.Zonas.Add(zonaMazo);
            jugador.Zonas.Add(zonaMano);
            jugador.Zonas.Add(zonaCampo);
            jugador.Zonas.Add(zonaDescarte);
            
            // Instanciamos las cartas del mazo
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
            
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Usuario.iniciarPartida.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
