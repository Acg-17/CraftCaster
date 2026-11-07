using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_ZonaPartida_operationTransaction) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.ZonaPartida;

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

public sealed partial class ZonaPartidaUseCase
{
    public async Task OperationTransactionAsync(int p_oid, int p_destino_oid, int index)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_ZonaPartida_operationTransaction) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        ZonaPartidaService zonaPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("ZonaPartidaUseCase.OperationTransactionAsync");
            zonaPartidaService = new ZonaPartidaService(_unitOfWork);

            // Custom Code:
            ZonaPartida zonaOrigen = await _unitOfWork.ZonaPartidaRepository.ReadAsync(p_oid) ?? throw new ModelException("No existe la zona de origen");
            ZonaPartida zonaDestino = await _unitOfWork.ZonaPartidaRepository.ReadAsync(p_destino_oid) ?? throw new ModelException("No existe la zona de destino");
        
            if(zonaOrigen.Cartas == null || zonaOrigen.Cartas.Count == 0)
            {
                throw new ModelException("No hay cartas en el origen para mover.");
            }

            // Tomamos la primera carta
            CartaPartida carta = zonaOrigen.Cartas[index];

            // Borramos carta de la zona
            zonaOrigen.Cartas.Remove(carta);

            // Le decimos cual es la nueva zona
            carta.Zona = zonaDestino;

            // La metemos en la zona destino
            zonaDestino.Cartas.Add(carta);

            // Guardamos base de datos
            await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaOrigen);
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDestino);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case ZonaPartida.operationTransaction.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
