var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');

Handlebars.registerHelper('if_windows', function(options) {
    if (StatusModel.get('isWindows')) {
        return options.fn(this);
    }

    return options.inverse(this);
});

Handlebars.registerHelper('if_mono', function(options) {
    if (StatusModel.get('isMono')) {
        return options.fn(this);
    }

    return options.inverse(this);
});