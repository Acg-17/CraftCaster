using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Usuario_abandonarPartida) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Usuario;

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


public sealed partial class UsuarioUseCase
{
    public async Task AbandonarPartidaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Usuario_abandonarPartida) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        UsuarioService usuarioService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("UsuarioUseCase.AbandonarPartidaAsync");
            usuarioService = new UsuarioService(_unitOfWork);

            Usuario usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el usuario indicado.");

            // Caso 1: El usuario es anfitrión (Usuario1 / PartidasLocal)
            Partida? partidaLocal = usuario.PartidasLocal.FirstOrDefault(p => p.Estado != EstadoPartida.Finalizada);
            if (partidaLocal != null)
            {
                // Si hay un jugador visitante, se desvincula de su lista
                if (partidaLocal.Usuario2 != null)
                {
                    partidaLocal.Usuario2.PartidasVisitante.Remove(partidaLocal);
                    await _unitOfWork.UsuarioRepository.SaveAsync(partidaLocal.Usuario2);
                }

                usuario.PartidasLocal.Remove(partidaLocal);
                await _unitOfWork.UsuarioRepository.SaveAsync(usuario);

                // Si la partida estaba en espera, se borra; si estaba en curso se finaliza dando victoria al visitante
                if (partidaLocal.Estado == EstadoPartida.Espera)
                {
                    // Las CartaPartida no se borran en cascada (relacion Share con ZonaPartida),
                    // asi que hay que borrarlas antes que sus zonas o falla la clave ajena.
                    foreach (var jugadorPartida in partidaLocal.Jugadores)
                    {
                        foreach (var zona in jugadorPartida.Zonas)
                        {
                            foreach (var cartaPartida in zona.Cartas.ToList())
                            {
                                await _unitOfWork.CartaPartidaRepository.DeleteAsync(cartaPartida);
                            }
                            zona.Cartas.Clear();
                        }
                    }

                    await _unitOfWork.PartidaRepository.DeleteAsync(partidaLocal);
                }
                else
                {
                    partidaLocal.Estado = EstadoPartida.Finalizada;
                    partidaLocal.Ganador = partidaLocal.Usuario2?.Id;
                    await _unitOfWork.PartidaRepository.SaveAsync(partidaLocal);
                }
            }
            // Caso 2: El usuario es visitante (Usuario2 / PartidasVisitante)
            else
            {
                Partida? partidaVisitante = usuario.PartidasVisitante.FirstOrDefault(p => p.Estado != EstadoPartida.Finalizada);
                if (partidaVisitante != null)
                {
                    usuario.PartidasVisitante.Remove(partidaVisitante);
                    await _unitOfWork.UsuarioRepository.SaveAsync(usuario);

                    partidaVisitante.Usuario2 = null;
                    partidaVisitante.Mazo2 = null;

                    // Si estaba en espera vuelve a quedar disponible; si estaba en curso gana el local por abandono
                    if (partidaVisitante.Estado == EstadoPartida.EnCurso)
                    {
                        partidaVisitante.Estado = EstadoPartida.Finalizada;
                        partidaVisitante.Ganador = partidaVisitante.Usuario1.Id;
                    }

                    await _unitOfWork.PartidaRepository.SaveAsync(partidaVisitante);
                }
                else
                {
                    throw new ModelException("El usuario no participa actualmente en ninguna partida activa.");
                }
            }

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case Usuario.abandonarPartida.", ex);
        }
        /*PROTECTED REGION END*/
    }
}