'use strict';
define(
    [
        'marionette',
        'handlebars',
        'underscore'
    ], function (Marionette, Handlebars, _) {

        var _fieldBuilder = function (field) {
            if (!field.type) {
                return _templateRenderer.apply(field,
                    [
                        'Form/TextboxTemplate'
                    ]);
            }

            if (field.type === 'password') {
                return _templateRenderer.apply(field,
                    [
                        'Form/PasswordTemplate'
                    ]);
            }

            if (field.type === 'checkbox') {
                return _templateRenderer.apply(field,
                    [
                        'Form/CheckboxTemplate'
                    ]);
            }

            if (field.type === 'select') {
                return _templateRenderer.apply(field,
                    [
                        'Form/SelectTemplate'
                    ]);
            }

            return _templateRenderer.apply(field,
                [
                    'Form/TextboxTemplate'
                ]);
        };

        var _templateRenderer = function (templateName) {
            var templateFunction = Marionette.TemplateCache.get(templateName);
            return new Handlebars.SafeString(templateFunction(this));
        };

        Handlebars.registerHelper('formBuilder', function () {
            var ret = '';
            _.each(this.fields, function (field) {
                ret += _fieldBuilder(field);
            });

            return new Handlebars.SafeString(ret);
        });
    });
