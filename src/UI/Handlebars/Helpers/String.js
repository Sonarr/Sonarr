var Handlebars = require('handlebars');

module.exports = (function(){
    Handlebars.registerHelper('TitleCase', function(input){
        return new Handlebars.SafeString(input.replace(/\w\S*/g, function(txt){
            return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
        }));
    });
}).call(this);