// Interacción del tablero TCG de CraftCaster: apuntado visual con flecha SVG,
// reglas de objetivos para criaturas e instantáneos, y polling ligero sin recargas innecesarias.
(function () {
    const arena = document.getElementById('ccArena');
    if (!arena) return;

    const esMiTurno = arena.dataset.esMiTurno === 'true';
    const criaturasRival = parseInt(arena.dataset.criaturasRival || '0', 10);
    const firmaInicial = arena.dataset.firma || '';
    const estadoUrl = arena.dataset.estadoUrl;
    const finUrl = arena.dataset.finUrl;

    const banner = document.getElementById('ccTargetingBanner');
    const bannerTitle = document.getElementById('ccTargetingTitle');
    const bannerHint = document.getElementById('ccTargetingHint');
    const btnCancelar = document.getElementById('ccCancelTargeting');
    const heroRival = document.getElementById('ccHeroRival');
    const heroRivalHint = document.getElementById('ccHeroRivalHint');
    const arrowSvg = document.getElementById('ccTargetArrowSvg');
    const arrowPath = document.getElementById('ccTargetArrowPath');

    const formLanzar = document.getElementById('formLanzarCarta');
    const hidLanzarCartaId = document.getElementById('hidLanzarCartaId');
    const hidLanzarObjetivoCartaId = document.getElementById('hidLanzarObjetivoCartaId');
    const hidLanzarObjetivoJugador = document.getElementById('hidLanzarObjetivoJugador');

    const formAtacar = document.getElementById('formAtacarCriatura');
    const hidAtacarAtacanteId = document.getElementById('hidAtacarAtacanteId');
    const hidAtacarObjetivoCartaId = document.getElementById('hidAtacarObjetivoCartaId');
    const hidAtacarObjetivoJugador = document.getElementById('hidAtacarObjetivoJugador');

    // Estado del apuntado activo: { accion: 'lanzar'|'atacar', sourceEl, cardId, allowEnemyCreature, allowOwnCreature, allowEnemyHero }
    let seleccion = null;
    let enviando = false;

    function limpiarApuntado() {
        seleccion = null;
        if (banner) banner.classList.add('d-none');
        if (arrowSvg) arrowSvg.classList.add('d-none');
        if (arrowPath) arrowPath.setAttribute('d', '');

        document.querySelectorAll('.cc-selected-source').forEach(el => el.classList.remove('cc-selected-source'));
        document.querySelectorAll('.cc-valid-target').forEach(el => el.classList.remove('cc-valid-target'));
        document.querySelectorAll('.cc-target-blocked').forEach(el => el.classList.remove('cc-target-blocked'));
        if (heroRivalHint) {
            heroRivalHint.classList.add('d-none');
            heroRivalHint.textContent = '';
        }
    }

    function actualizarFlecha(clientX, clientY) {
        if (!seleccion || !arrowSvg || !arrowPath) return;
        const rect = seleccion.sourceEl.getBoundingClientRect();
        const x1 = rect.left + rect.width / 2;
        const y1 = rect.top + rect.height / 2;
        const x2 = clientX;
        const y2 = clientY;

        const cx = (x1 + x2) / 2;
        const cy = Math.min(y1, y2) - 55;
        arrowPath.setAttribute('d', `M ${x1} ${y1} Q ${cx} ${cy} ${x2} ${y2}`);
        arrowSvg.classList.remove('d-none');
    }

    function iniciarApuntado(config) {
        limpiarApuntado();
        seleccion = config;
        config.sourceEl.classList.add('cc-selected-source');

        if (banner && bannerTitle && bannerHint) {
            bannerTitle.textContent = config.titulo;
            bannerHint.textContent = config.pista;
            banner.classList.remove('d-none');
        }

        if (config.allowEnemyCreature) {
            document.querySelectorAll('[data-enemy-creature="true"]').forEach(el => el.classList.add('cc-valid-target'));
        }
        if (config.allowOwnCreature) {
            document.querySelectorAll('[data-own-creature="true"]').forEach(el => el.classList.add('cc-valid-target'));
        }
        if (heroRival) {
            if (config.allowEnemyHero) {
                heroRival.classList.add('cc-valid-target');
                if (heroRivalHint) {
                    heroRivalHint.textContent = 'Clic aquí para apuntar al jugador rival';
                    heroRivalHint.classList.remove('d-none');
                }
            } else if (config.heroBlockedReason) {
                heroRival.classList.add('cc-target-blocked');
                if (heroRivalHint) {
                    heroRivalHint.textContent = config.heroBlockedReason;
                    heroRivalHint.classList.remove('d-none');
                }
            }
        }
    }

    function enviarLanzar(cartaId, objetivoCartaId, objetivoJugador) {
        if (enviando || !formLanzar) return;
        enviando = true;
        hidLanzarCartaId.value = cartaId;
        hidLanzarObjetivoCartaId.value = objetivoCartaId || '';
        hidLanzarObjetivoJugador.value = objetivoJugador ? 'true' : 'false';
        formLanzar.submit();
    }

    function enviarAtacar(atacanteId, objetivoCartaId, objetivoJugador) {
        if (enviando || !formAtacar) return;
        enviando = true;
        hidAtacarAtacanteId.value = atacanteId;
        hidAtacarObjetivoCartaId.value = objetivoCartaId || '';
        hidAtacarObjetivoJugador.value = objetivoJugador ? 'true' : 'false';
        formAtacar.submit();
    }

    // 1. Clic en cartas de la mano (lanzar o abrir apuntado)
    document.querySelectorAll('[data-card-zone="Mano"][data-playable="true"]').forEach(cardEl => {
        cardEl.addEventListener('click', (e) => {
            e.stopPropagation();
            if (!esMiTurno || enviando) return;

            const cardId = cardEl.dataset.cardId;
            const nombre = cardEl.dataset.cardName || 'Carta';
            const modo = parseInt(cardEl.dataset.targetMode || '0', 10);

            if (seleccion && seleccion.sourceEl === cardEl) {
                limpiarApuntado();
                return;
            }

            // Modo 0: Ninguno -> lanzar directamente
            if (modo === 0) {
                enviarLanzar(cardId, null, false);
                return;
            }

            // Modo 1: EnemigoOCriatura (instantáneo o criatura al entrar)
            if (modo === 1) {
                iniciarApuntado({
                    accion: 'lanzar',
                    sourceEl: cardEl,
                    cardId,
                    titulo: `Apuntando con ${nombre}`,
                    pista: 'Elige una criatura rival en mesa o al jugador enemigo.',
                    allowEnemyCreature: criaturasRival > 0,
                    allowOwnCreature: false,
                    allowEnemyHero: true,
                    heroBlockedReason: null
                });
                return;
            }

            // Modo 2: SoloCriaturaEnemiga (p. ej. Destruir enemigo)
            if (modo === 2) {
                iniciarApuntado({
                    accion: 'lanzar',
                    sourceEl: cardEl,
                    cardId,
                    titulo: `Elige criatura rival para ${nombre}`,
                    pista: 'Haz clic sobre la criatura rival objetivo en la mesa.',
                    allowEnemyCreature: true,
                    allowOwnCreature: false,
                    allowEnemyHero: false,
                    heroBlockedReason: 'Esta carta solo puede apuntar a criaturas rivales.'
                });
                return;
            }

            // Modo 3: SoloCriaturaPropia
            if (modo === 3) {
                iniciarApuntado({
                    accion: 'lanzar',
                    sourceEl: cardEl,
                    cardId,
                    titulo: `Elige una de tus criaturas para ${nombre}`,
                    pista: 'Haz clic sobre una criatura de tu mesa.',
                    allowEnemyCreature: false,
                    allowOwnCreature: true,
                    allowEnemyHero: false,
                    heroBlockedReason: null
                });
                return;
            }

            // Modo 4: SoloJugadorEnemigo (hechizo directo al rival)
            if (modo === 4) {
                iniciarApuntado({
                    accion: 'lanzar',
                    sourceEl: cardEl,
                    cardId,
                    titulo: `Lanzar ${nombre} al jugador rival`,
                    pista: 'Haz clic en el corazón de vida del rival para confirmar.',
                    allowEnemyCreature: false,
                    allowOwnCreature: false,
                    allowEnemyHero: true,
                    heroBlockedReason: null
                });
            }
        });
    });

    // 2. Clic en criaturas propias listas para atacar en mesa
    document.querySelectorAll('[data-card-zone="CampoPropio"]').forEach(cardEl => {
        cardEl.addEventListener('click', (e) => {
            e.stopPropagation();
            if (!esMiTurno || enviando) return;

            // Si estamos en modo apuntado hacia criatura propia (p. ej. Destruir propia)
            if (seleccion && seleccion.allowOwnCreature && cardEl.classList.contains('cc-valid-target')) {
                enviarLanzar(seleccion.cardId, cardEl.dataset.cardId, false);
                return;
            }

            if (cardEl.dataset.canAttack !== 'true') return;

            if (seleccion && seleccion.sourceEl === cardEl) {
                limpiarApuntado();
                return;
            }

            const cardId = cardEl.dataset.cardId;
            const nombre = cardEl.dataset.cardName || 'Criatura';
            const hayBloqueadoras = criaturasRival > 0;

            iniciarApuntado({
                accion: 'atacar',
                sourceEl: cardEl,
                cardId,
                titulo: `Atacando con ${nombre}`,
                pista: hayBloqueadoras
                    ? 'Debes elegir una criatura rival en la mesa (no puedes atacar al jugador mientras queden criaturas).'
                    : 'La mesa rival está vacía: haz clic en el jugador rival para atacar directamente.',
                allowEnemyCreature: hayBloqueadoras,
                allowOwnCreature: false,
                allowEnemyHero: !hayBloqueadoras,
                heroBlockedReason: hayBloqueadoras
                    ? 'Protegido: elimina primero las criaturas rivales en mesa'
                    : null
            });
        });
    });

    // 3. Clic en criaturas rivales como objetivo
    document.querySelectorAll('[data-enemy-creature="true"]').forEach(enemyEl => {
        enemyEl.addEventListener('click', (e) => {
            e.stopPropagation();
            if (!seleccion || !seleccion.allowEnemyCreature || enviando) return;

            const objetivoId = enemyEl.dataset.cardId;
            if (seleccion.accion === 'atacar') {
                enviarAtacar(seleccion.cardId, objetivoId, false);
            } else {
                enviarLanzar(seleccion.cardId, objetivoId, false);
            }
        });
    });

    // 4. Clic en el jugador rival (HUD corazón) como objetivo
    if (heroRival) {
        heroRival.addEventListener('click', (e) => {
            e.stopPropagation();
            if (!seleccion || enviando) return;
            if (!seleccion.allowEnemyHero) return;

            if (seleccion.accion === 'atacar') {
                enviarAtacar(seleccion.cardId, null, true);
            } else {
                enviarLanzar(seleccion.cardId, null, true);
            }
        });
    }

    // Seguimiento de flecha y cancelación
    document.addEventListener('mousemove', (e) => {
        if (seleccion) actualizarFlecha(e.clientX, e.clientY);
    });

    if (btnCancelar) {
        btnCancelar.addEventListener('click', (e) => {
            e.stopPropagation();
            limpiarApuntado();
        });
    }

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && seleccion) limpiarApuntado();
    });

    document.addEventListener('click', () => {
        if (seleccion) limpiarApuntado();
    });

    // 5. Polling ligero en segundo plano: solo recarga cuando el estado de la partida cambia de verdad
    if (estadoUrl) {
        setInterval(async () => {
            if (enviando || seleccion) return;
            try {
                const resp = await fetch(estadoUrl, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    cache: 'no-store'
                });
                if (!resp.ok) return;
                const data = await resp.json();
                if (data.estado === 'Finalizada' && finUrl) {
                    window.location.href = finUrl;
                    return;
                }
                if (data.firma && data.firma !== firmaInicial) {
                    window.location.reload();
                }
            } catch {
                // Silencioso en caso de pérdida momentánea de red
            }
        }, 2500);
    }
})();
