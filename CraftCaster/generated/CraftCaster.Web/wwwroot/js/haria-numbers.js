// HARIA-WEB GENERATED sha256:7cd18924613a8c64e99c5f218539834d55566175d3cc40eae50d57a49842b76e
(function (global) {
    "use strict";
    function parseNumber(value, separator) {
        var text = String(value).trim();
        var escaped = separator.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
        var pattern = new RegExp("^[+-]?(?:[0-9]+(?:" + escaped + "[0-9]*)?|" + escaped + "[0-9]+)(?:[eE][+-]?[0-9]+)?$");
        if (!pattern.test(text)) return NaN;
        var number = Number(text.replace(separator, "."));
        return Number.isFinite(number) ? number : NaN;
    }
    global.HariaNumbers = { parseNumber: parseNumber };
    var $ = global.jQuery;
    if (!$ || !$.validator) return;
    ["number", "range", "min", "max"].forEach(function (rule) {
        var original = $.validator.methods[rule];
        $.validator.methods[rule] = function (value, element, parameters) {
            if (!element.hasAttribute("data-haria-number")) return original.call(this, value, element, parameters);
            if (this.optional(element)) return true;
            var number = parseNumber(value, element.getAttribute("data-haria-decimal") || ".");
            if (!Number.isFinite(number)) return false;
            if (rule === "range") return number >= Number(parameters[0]) && number <= Number(parameters[1]);
            if (rule === "min") return number >= Number(parameters);
            if (rule === "max") return number <= Number(parameters);
            return true;
        };
    });
})(window);
