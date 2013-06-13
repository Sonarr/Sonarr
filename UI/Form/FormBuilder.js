'use strict';
define(['app'], function () {
    Handlebars.registerHelper('formBuilder', function (){
        var ret = "";
        _.each(this.fields, function(field){
            ret += NzbDrone.Form.FieldBuilder(field);
        });

        return new Handlebars.SafeString(ret);
    });

    NzbDrone.Form.FieldBuilder = function(field) {
        if (!field.type) {
            return Handlebars.helpers.partial.apply(field, ['Form/TextboxTemplate']);
        }

        if (field.type === 'password') {
            return Handlebars.helpers.partial.apply(field, ['Form/PasswordTemplate']);
        }

        if (field.type === 'checkbox') {
            return Handlebars.helpers.partial.apply(field, ['Form/CheckboxTemplate']);
        }

        if (field.type === 'select') {
            return Handlebars.helpers.partial.apply(field, ['Form/SelectTemplate']);
        }

        return Handlebars.helpers.partial.apply(field, ['Form/TextboxTemplate']);
    };
});
