var Marionette = require('marionette');
var Handlebars = require('handlebars');
var _ = require('underscore');
require('./FormMessage');

var _templateRenderer = function(templateName) {
    var templateFunction = Marionette.TemplateCache.get(templateName);
    return new Handlebars.SafeString(templateFunction(this));
};

var _fieldBuilder = function(field) {
    if (!field.type) {
        return _templateRenderer.call(field, 'Form/TextboxTemplate');
    }

    if (field.type === 'hidden') {
        return _templateRenderer.call(field, 'Form/HiddenTemplate');
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

    if (field.type === 'hidden') {
        return _templateRenderer.call(field, 'Form/HiddenTemplate');
    }

    if (field.type === 'path') {
        return _templateRenderer.call(field, 'Form/PathTemplate');
    }

    if (field.type === 'tag') {
        return _templateRenderer.call(field, 'Form/TagTemplate');
    }

    if (field.type === 'action') {
        return _templateRenderer.call(field, 'Form/ActionTemplate');
    }


    return _templateRenderer.call(field, 'Form/TextboxTemplate');
};

Handlebars.registerHelper('formBuilder', function() {
    var ret = '';
    _.each(this.fields, function(field) {
        ret += _fieldBuilder(field);
    });

    return new Handlebars.SafeString(ret);
});