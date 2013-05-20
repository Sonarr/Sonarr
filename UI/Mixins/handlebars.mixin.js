'use strict';

Handlebars.registerHelper('partial', function (templateName) {
    //TODO: We should be able to pass in the context, either an object or a property

    var templateFunction = Marionette.TemplateCache.get(templateName);
    return new Handlebars.SafeString(templateFunction(this));
});

Handlebars.registerHelper('formField', function () {
    if (!this.type) {
        return Handlebars.helpers.partial.apply(this, ['Form/TextboxTemplate']);
    }

    if (this.type === 'password') {
        return Handlebars.helpers.partial.apply(this, ['Form/PasswordTemplate']);
    }

    if (this.type === 'checkbox') {
        return Handlebars.helpers.partial.apply(this, ['Form/CheckboxTemplate']);
    }

    return Handlebars.helpers.partial.apply(this, ['Form/TextboxTemplate']);
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
