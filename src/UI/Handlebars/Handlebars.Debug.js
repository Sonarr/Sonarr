var Handlebars = require('handlebars');

module.exports = (function(){
    Handlebars.registerHelper('debug', function(){
        console.group('Handlebar context');
        console.log(this);
        console.groupEnd();
    });
}).call(this);