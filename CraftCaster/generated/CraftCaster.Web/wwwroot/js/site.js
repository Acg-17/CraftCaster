// Conservar la posición de scroll tras enviar formularios POST que recargan la misma vista
(function () {
    const key = 'cc_scroll:' + window.location.pathname;
    const saved = sessionStorage.getItem(key);
    if (saved !== null) {
        sessionStorage.removeItem(key);
        const y = parseInt(saved, 10);
        if (!Number.isNaN(y) && y > 0) {
            window.scrollTo(0, y);
        }
    }

    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form && form.method && form.method.toLowerCase() === 'post') {
            sessionStorage.setItem(key, String(window.scrollY));
        }
    });
})();
