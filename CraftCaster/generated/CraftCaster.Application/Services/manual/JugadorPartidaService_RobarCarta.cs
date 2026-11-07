using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_JugadorPartida_robarCarta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class JugadorPartidaService
{
    public async Task RobarCartaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_JugadorPartida_robarCarta) ENABLED START*/
        const int maxCartasMano = 10;

        // Comprobar que el jugador existe
        JugadorPartida jugador = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe el jugador indicado.");

        // Comprobar que la partida no ha finalizado
        if (jugador.Partida != null && jugador.Partida.Estado == EstadoPartida.Finalizada)
            throw new ModelException("La partida ya ha finalizado, no se pueden robar cartas.");

        // Comprobar que el jugador tiene zonas de mazo y mano
        ZonaPartida zonaMazo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo)
            ?? throw new ModelException("El jugador no dispone de zona de mazo.");
        ZonaPartida zonaMano = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mano)
            ?? throw new ModelException("El jugador no dispone de zona de mano.");

        // Comprobar que el mazo no está vacío
        if (zonaMazo.Cartas == null || zonaMazo.Cartas.Count == 0)
            throw new ModelException("El mazo esta vacio, no quedan cartas para robar.");

        CartaPartida carta = zonaMazo.Cartas[0];
        zonaMazo.Cartas.Remove(carta);

        // Comprobar si la mano está llena
        ZonaPartida zonaDestino = zonaMano;
        if (zonaMano.Cartas != null && zonaMano.Cartas.Count >= maxCartasMano)
        {
            zonaDestino = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Descarte)
                ?? throw new ModelException("La mano esta llena y el jugador no dispone de zona de descarte.");
        }

        // Mover la carta a la zona de destino
        carta.Zona = zonaDestino;
        zonaDestino.Cartas ??= new List<CartaPartida>();
        zonaDestino.Cartas.Add(carta);

        await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
        await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDestino);
        await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaMazo);
        /*PROTECTED REGION END*/
    }
}
