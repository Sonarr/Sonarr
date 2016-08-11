var Handlebars = require('handlebars');

Handlebars.registerHelper('meta-with', function(context, options) {

    if (!options)
        return '';

    context.metaOption = context.metaOption || {};
    context.metaOption.tab = options.hash.tab;
    context.metaOption.option = options.hash.option;

    return options.fn(context);
});

Handlebars.registerHelper('meta-ro', function(context, options) {

    if (!options) {
        return '';
    }

    if (context.metaOption) {
        var tab = context.metaOption.tab;
        var opt = context.metaOption.option;

console.log('meta-ro context', context);
console.log(tab, opt);

        return options.fn(context);
    } else {
        return options.fn(context);
    }
});

Handlebars.registerHelper('meta-show', function(context, options) {

    if (!options) {
        return '';
    }

    if (context.metaOption) {
        var tab = context.metaOption.tab;
        var opt = context.metaOption.option;

console.log('meta-show context', context);
console.log(tab, opt);

        return options.fn(context);
    } else {
        return options.fn(context);
    }
});
