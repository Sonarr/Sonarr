var Handlebars = require('handlebars');

Handlebars.registerHelper('times', function(n, block) {
    var accum = '';

    for (var i = 0; i < n; ++i) {
        accum += block.fn(i);
    }

    return accum;
});

Handlebars.registerHelper('for', function(from, to, incr, block) {
    var accum = '';

    for (var i = from; i < to; i += incr) {
        accum += block.fn(i);
    }

    return accum;
});