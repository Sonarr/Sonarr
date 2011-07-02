/*
* jQuery doTimeout: Like setTimeout, but better! - v1.0 - 3/3/2010
* http://benalman.com/projects/jquery-dotimeout-plugin/
* 
* Copyright (c) 2010 "Cowboy" Ben Alman
* Dual licensed under the MIT and GPL licenses.
* http://benalman.com/about/license/
*/
(function ($) { var a = {}, c = "doTimeout", d = Array.prototype.slice; $[c] = function () { return b.apply(window, [0].concat(d.call(arguments))) }; $.fn[c] = function () { var f = d.call(arguments), e = b.apply(this, [c + f[0]].concat(f)); return typeof f[0] === "number" || typeof f[1] === "number" ? this : e }; function b(l) { var m = this, h, k = {}, g = l ? $.fn : $, n = arguments, i = 4, f = n[1], j = n[2], p = n[3]; if (typeof f !== "string") { i--; f = l = 0; j = n[1]; p = n[2] } if (l) { h = m.eq(0); h.data(l, k = h.data(l) || {}) } else { if (f) { k = a[f] || (a[f] = {}) } } k.id && clearTimeout(k.id); delete k.id; function e() { if (l) { h.removeData(l) } else { if (f) { delete a[f] } } } function o() { k.id = setTimeout(function () { k.fn() }, j) } if (p) { k.fn = function (q) { if (typeof p === "string") { p = g[p] } p.apply(m, d.call(n, i)) === true && !q ? o() : e() }; o() } else { if (k.fn) { j === undefined ? e() : k.fn(j === false); return true } else { e() } } } })(jQuery);