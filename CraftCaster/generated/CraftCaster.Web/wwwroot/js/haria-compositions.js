// HARIA-WEB GENERATED sha256:b667dee477ede0a6ac394f9eb47f6a3b1bd5e202f92428353eb96a4d4e379697
document.querySelectorAll('[data-haria-composition]').forEach(container => {
  if (container.dataset.initialized) return;
  container.dataset.initialized = 'true';
  const rows = container.querySelector('[data-haria-rows]');
  const prefix = container.dataset.prefix;
  function reindex() {
    [...rows.children].forEach((row, index) => {
      const ids = new Map();
      row.querySelectorAll('[name]').forEach(field => {
        const suffix = field.name.slice(field.name.indexOf(']') + 1);
        field.name = prefix + '[' + index + ']' + suffix;
        const id = field.name.replaceAll('.', '_').replaceAll('[', '_').replaceAll(']', '_');
        if (field.id) { ids.set(field.id, id); field.id = id; }
      });
      row.querySelectorAll('label[for]').forEach(label => { if (ids.has(label.htmlFor)) label.htmlFor = ids.get(label.htmlFor); });
      row.querySelectorAll('[data-valmsg-for]').forEach(span => {
        const value = span.getAttribute('data-valmsg-for');
        span.setAttribute('data-valmsg-for', prefix + '[' + index + ']' + value.slice(value.indexOf(']') + 1));
      });
    });
    const form = container.closest('form');
    if (window.jQuery?.validator?.unobtrusive) {
      window.jQuery(form).removeData('validator').removeData('unobtrusiveValidation');
      window.jQuery.validator.unobtrusive.parse(form);
    }
  }
  container.querySelector('[data-haria-add]').addEventListener('click', () => {
    if (rows.children.length >= 500) return;
    rows.append(container.querySelector('template').content.cloneNode(true));
    reindex();
  });
  rows.addEventListener('click', event => {
    if (event.target.closest('[data-haria-remove]')) { event.target.closest('[data-haria-row]').remove(); reindex(); }
  });
  container.closest('form').addEventListener('submit', reindex);
});
