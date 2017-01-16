var Handlebars = require('handlebars');

Handlebars.registerHelper('debug', function() {
    console.group('Handlebar context');
    console.log(this);
    console.groupEnd();
});