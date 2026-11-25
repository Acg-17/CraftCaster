// Alterna automáticamente el círculo (Maná) y el corazón (Vida) en todas las cartas de tipo Criatura
(function () {
    function alternarTarjeta(card) {
        const mana = card.querySelector('.cc-mana-circle');
        const heart = card.querySelector('.cc-heart-wrap');
        if (!mana || !heart) return;
        mana.classList.toggle('d-none');
        heart.classList.toggle('d-none');
    }

    const criaturas = document.querySelectorAll('.cc-card-criatura');
    if (criaturas.length === 0) return;

    criaturas.forEach(card => {
        const slot = card.querySelector('.cc-toggle-mana-vida');
        if (slot) {
            slot.addEventListener('click', () => alternarTarjeta(card));
        }
    });

    setInterval(() => {
        criaturas.forEach(alternarTarjeta);
    }, 2500);
})();
