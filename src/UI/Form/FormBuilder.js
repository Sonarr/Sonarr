var Marionette = require('marionette');
var Handlebars = require('handlebars');
var _ = require('underscore');

var _fieldBuilder = function(field) {
    if (!field.type) {
        return _templateRenderer.call(field, 'Form/TextboxTemplate');
    }

    if (field.type === 'password') {
        return _templateRenderer.call(field, 'Form/PasswordTemplate');
    }

    if (field.type === 'checkbox') {
        return _templateRenderer.call(field, 'Form/CheckboxTemplate');
    }

    if (field.type === 'select') {
        return _templateRenderer.call(field, 'Form/SelectTemplate');
    }

    if (field.type === 'path') {
        return _templateRenderer.call(field, 'Form/PathTemplate');
    }

    return _templateRenderer.call(field, 'Form/TextboxTemplate');
};

var _templateRenderer = function(templateName) {
    var templateFunction = Marionette.TemplateCache.get(templateName);
    return new Handlebars.SafeString(templateFunction(this));
};

Handlebars.registerHelper('formBuilder', function() {
    var ret = '';
    _.each(this.fields, function(field) {
        ret += _fieldBuilder(field);
    });

    return new Handlebars.SafeString(ret);
});