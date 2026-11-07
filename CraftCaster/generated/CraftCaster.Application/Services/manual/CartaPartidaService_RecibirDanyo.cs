using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_CartaPartida_recibirDanyo) ENABLED START*/
        using System.Collections.Generic;
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class CartaPartidaService
{
    public async Task RecibirDanyoAsync(int p_oid, int p_danyo)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_CartaPartida_recibirDanyo) ENABLED START*/
        if (p_danyo < 0)
        {
            throw new ModelException("El danyo no puede ser negativo.");
        }

        CartaPartida carta = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException($"No existe la carta en partida con id {p_oid}.");

        carta.VidaActual -= p_danyo;

        if (carta.VidaActual <= 0)
        {
            carta.VidaActual = 0;
            carta.PuedeAtacar = false;

            ZonaPartida? zonaActual = carta.Zona;
            JugadorPartida? jugador = zonaActual?.Jugador;

            if (jugador != null)
            {
                ZonaPartida? zonaDescarte = jugador.Zonas?.FirstOrDefault(z => z.TipoZona == TipoZona.Descarte);
                if (zonaDescarte != null && zonaActual != null && zonaActual.Id != zonaDescarte.Id)
                {
                    zonaActual.Cartas?.Remove(carta);
                    carta.Zona = zonaDescarte;
                    zonaDescarte.Cartas ??= new List<CartaPartida>();
                    if (!zonaDescarte.Cartas.Contains(carta))
                    {
                        zonaDescarte.Cartas.Add(carta);
                    }

                    await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaActual);
                    await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDescarte);
                }
            }
        }

        await _repository.SaveAsync(carta);
        /*PROTECTED REGION END*/
    }
}
