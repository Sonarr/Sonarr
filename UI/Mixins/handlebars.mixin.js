'use strict';

Handlebars.registerHelper('partial', function(templateName, context){
    //TODO: We should be able to pass in the context, either an object or a property

    var templateFunction = Marionette.TemplateCache.get(templateName);
    return new Handlebars.SafeString(templateFunction(this));
});

Handlebars.registerHelper("debug", function(optionalValue) {
    console.log("Current Context");
    console.log("====================");
    console.log(this);

    if (optionalValue) {
        console.log("Value");
        console.log("====================");
        console.log(optionalValue);
    }
});