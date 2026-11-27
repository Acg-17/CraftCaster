using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Habilidad_ejecutar) ENABLED START*/
        // references to other libraries
        using System.Collections.Generic;
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class HabilidadService
{
    public async Task<string> EjecutarAsync(int p_oid, int p_origen_oid, int p_objetivoCarta, bool p_objetivoJugador)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Habilidad_ejecutar) ENABLED START*/
        // Ejecuta una habilidad desde la carta que la contiene (p_origen_oid), SIN abrir transacción:
        // la usan las CustomTransaction lanzarCarta, atacar y pasarTurno dentro de la suya,
        // así cada acción del jugador es atómica. p_objetivoCarta = 0 si no se ha elegido carta objetivo.
        // Devuelve el resumen de lo ocurrido para mostrarlo en el tablero.
        Habilidad habilidad = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe la habilidad indicada.");
        CartaPartida origen = await _unitOfWork.CartaPartidaRepository.ReadAsync(p_origen_oid)
            ?? throw new ModelException("No existe la carta que activa la habilidad.");
        JugadorPartida dueno = origen.Zona?.Jugador
            ?? throw new ModelException("La carta que activa la habilidad no está en ninguna zona.");
        Partida partida = dueno.Partida
            ?? throw new ModelException("La carta que activa la habilidad no está en ninguna partida.");
        if (partida.Estado != EstadoPartida.EnCurso)
            throw new ModelException("Solo se pueden ejecutar habilidades en una partida en curso.");
        JugadorPartida rival = partida.Jugadores.FirstOrDefault(j => j.Id != dueno.Id)
            ?? throw new ModelException("La partida no tiene rival.");

        int? objetivoCartaId = p_objetivoCarta > 0 ? p_objetivoCarta : null;
        var resumen = new List<string>();
        var jugadorService = new JugadorPartidaService(_unitOfWork);
        var cartaPartidaService = new CartaPartidaService(_unitOfWork);

        ZonaPartida? ZonaDe(JugadorPartida jugador, TipoZona tipo) =>
            jugador.Zonas?.FirstOrDefault(z => z.TipoZona == tipo);

        // Habilidades "Al morir" de una criatura destruida (cada una con esta misma operación)
        async Task DispararAlMorirAsync(CartaPartida criatura)
        {
            var habilidadesMorir = (criatura.Original.Estados ?? new List<Estado>())
                .Where(e => e.Tipo == TipoEstado.Morir)
                .SelectMany(e => e.Habilidades ?? new List<Habilidad>())
                .Where(h => h.Tipo != TipoHabilidad.Defensa)
                .ToList();
            foreach (var habilidadMorir in habilidadesMorir)
            {
                string efecto = await EjecutarAsync(habilidadMorir.Id, criatura.Id, 0, false);
                if (!string.IsNullOrWhiteSpace(efecto)) resumen.Add(efecto);
            }
        }

        // Daño a una criatura en la mesa con CartaPartida.recibirDanyo (que la manda al descarte si muere).
        // Solo las criaturas que están en la mesa reciben daño, así cada criatura muere una sola vez.
        async Task<bool> AplicarDanyoAsync(CartaPartida criatura, int danyo)
        {
            if (danyo <= 0 || criatura.Zona?.TipoZona != TipoZona.Campo || criatura.VidaActual <= 0) return false;
            await cartaPartidaService.RecibirDanyoAsync(criatura.Id, danyo);
            if (criatura.VidaActual > 0) return false;
            await DispararAlMorirAsync(criatura);
            return true;
        }

        async Task DestruirAsync(JugadorPartida duenoCriatura, CartaPartida criatura)
        {
            criatura.VidaActual = 0;
            criatura.PuedeAtacar = false;
            var zonaActual = criatura.Zona;
            var zonaDescarte = ZonaDe(duenoCriatura, TipoZona.Descarte);
            if (zonaActual is not null && zonaDescarte is not null && zonaActual.Id != zonaDescarte.Id)
            {
                zonaActual.Cartas.Remove(criatura);
                criatura.Zona = zonaDescarte;
                if (!zonaDescarte.Cartas.Contains(criatura)) zonaDescarte.Cartas.Add(criatura);
                await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaActual);
                await _unitOfWork.ZonaPartidaRepository.SaveAsync(zonaDescarte);
            }
            await _unitOfWork.CartaPartidaRepository.SaveAsync(criatura);
            await DispararAlMorirAsync(criatura);
        }

        var campoRival = ZonaDe(rival, TipoZona.Campo);
        var campoPropio = ZonaDe(dueno, TipoZona.Campo);

        switch (habilidad.Tipo)
        {
            case TipoHabilidad.Danyo:
            case TipoHabilidad.Ataque:
                if (habilidad.TipoObjetivo == TipoObjetivo.TodosEnemigos)
                {
                    var objetivos = campoRival?.Cartas.ToList() ?? new List<CartaPartida>();
                    foreach (var criatura in objetivos)
                    {
                        await AplicarDanyoAsync(criatura, habilidad.Cantidad);
                    }
                    if (objetivos.Count > 0)
                    {
                        resumen.Insert(0, $"Inflige {habilidad.Cantidad} de daño a todas las criaturas rivales.");
                    }
                }
                else if (habilidad.TipoObjetivo == TipoObjetivo.Jugador || p_objetivoJugador)
                {
                    await jugadorService.ActualizarVidaAsync(rival.Id, habilidad.Cantidad);
                    resumen.Insert(0, $"Inflige {habilidad.Cantidad} de daño al rival.");
                }
                else
                {
                    // Enemigo o Libre: el objetivo elegido o, si no hay, una criatura rival al azar o el jugador rival
                    CartaPartida? objetivo = objetivoCartaId is int idObjetivo
                        ? campoRival?.Cartas.FirstOrDefault(c => c.Id == idObjetivo)
                        : null;
                    if (objetivo is null && campoRival is not null && campoRival.Cartas.Count > 0)
                    {
                        objetivo = campoRival.Cartas[Random.Shared.Next(campoRival.Cartas.Count)];
                    }

                    if (objetivo is not null && objetivo.Zona?.TipoZona == TipoZona.Campo && objetivo.VidaActual > 0)
                    {
                        string nombreObjetivo = objetivo.Original.Nombre;
                        bool murio = await AplicarDanyoAsync(objetivo, habilidad.Cantidad);
                        resumen.Insert(0, murio
                            ? $"Inflige {habilidad.Cantidad} de daño a {nombreObjetivo} y la destruye."
                            : $"Inflige {habilidad.Cantidad} de daño a {nombreObjetivo}.");
                    }
                    else
                    {
                        await jugadorService.ActualizarVidaAsync(rival.Id, habilidad.Cantidad);
                        resumen.Insert(0, $"Inflige {habilidad.Cantidad} de daño al rival.");
                    }
                }
                break;

            case TipoHabilidad.Cura:
                if (habilidad.TipoObjetivo == TipoObjetivo.Self && origen.Original.Tipo == TipoCarta.Criatura && origen.Zona?.TipoZona == TipoZona.Campo)
                {
                    origen.VidaActual += habilidad.Cantidad;
                    await _unitOfWork.CartaPartidaRepository.SaveAsync(origen);
                    resumen.Add($"{origen.Original.Nombre} recupera {habilidad.Cantidad} de vida.");
                }
                else
                {
                    // actualizarVida resta: con cantidad negativa suma vida al jugador
                    await jugadorService.ActualizarVidaAsync(dueno.Id, -habilidad.Cantidad);
                    resumen.Add($"Suma {habilidad.Cantidad} de vida al jugador.");
                }
                break;

            case TipoHabilidad.Robar:
                var mazoDueno = ZonaDe(dueno, TipoZona.Mazo);
                int robadas = 0;
                for (int i = 0; i < habilidad.Cantidad; i++)
                {
                    if (mazoDueno is null || mazoDueno.Cartas.Count == 0) break;
                    await jugadorService.RobarCartaDelMazoAsync(dueno.Id);
                    robadas++;
                }
                if (robadas > 0) resumen.Add($"Roba {robadas} carta(s).");
                break;

            case TipoHabilidad.Descartar:
                var manoRival = ZonaDe(rival, TipoZona.Mano);
                var descarteRival = ZonaDe(rival, TipoZona.Descarte);
                if (manoRival is not null && descarteRival is not null && manoRival.Cartas.Count > 0)
                {
                    int descartadas = 0;
                    for (int i = 0; i < habilidad.Cantidad && manoRival.Cartas.Count > 0; i++)
                    {
                        var elegida = manoRival.Cartas[Random.Shared.Next(manoRival.Cartas.Count)];
                        manoRival.Cartas.Remove(elegida);
                        elegida.Zona = descarteRival;
                        descarteRival.Cartas.Add(elegida);
                        await _unitOfWork.CartaPartidaRepository.SaveAsync(elegida);
                        descartadas++;
                    }
                    await _unitOfWork.ZonaPartidaRepository.SaveAsync(manoRival);
                    await _unitOfWork.ZonaPartidaRepository.SaveAsync(descarteRival);
                    if (descartadas > 0) resumen.Add($"El rival descarta {descartadas} carta(s) de su mano.");
                }
                break;

            case TipoHabilidad.Destruir:
                // Objetivo Jugador: destruye una criatura propia; si no, una rival
                bool destruirPropia = habilidad.TipoObjetivo == TipoObjetivo.Jugador;
                var zonaObjetivo = destruirPropia ? campoPropio : campoRival;
                var duenoObjetivo = destruirPropia ? dueno : rival;
                if (zonaObjetivo is not null && zonaObjetivo.Cartas.Count > 0)
                {
                    CartaPartida? victima = objetivoCartaId is int idVictima
                        ? zonaObjetivo.Cartas.FirstOrDefault(c => c.Id == idVictima)
                        : null;
                    victima ??= zonaObjetivo.Cartas[Random.Shared.Next(zonaObjetivo.Cartas.Count)];
                    string nombreVictima = victima.Original.Nombre;
                    await DestruirAsync(duenoObjetivo, victima);
                    resumen.Insert(0, $"Destruye a {nombreVictima}.");
                }
                break;

            case TipoHabilidad.Defensa:
                // Pasiva: devuelve daño cuando la criatura es atacada (lo aplica JugadorPartida.atacar)
                break;
        }

        return string.Join(" ", resumen);
        /*PROTECTED REGION END*/
    }
}
