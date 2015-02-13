var Handlebars = require('handlebars');

Handlebars.registerHelper('TitleCase', function(input) {
    return new Handlebars.SafeString(input.replace(/\w\S*/g, function(txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    }));
});