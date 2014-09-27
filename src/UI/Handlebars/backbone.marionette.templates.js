'use strict';
define(
    [
        'templates',
        'handlebars',
        'handlebars.helpers',
        'Handlebars/Helpers/DateTime',
        'Handlebars/Helpers/Html',
        'Handlebars/Helpers/Numbers',
        'Handlebars/Helpers/Episode',
        'Handlebars/Helpers/Series',
        'Handlebars/Helpers/Quality',
        'Handlebars/Helpers/System',
        'Handlebars/Helpers/EachReverse',
        'Handlebars/Helpers/String',
        'Handlebars/Handlebars.Debug'
    ], function (Templates, Handlebars) {
        return function () {
            this.get = function (templateId) {
                var templateKey = templateId.toLowerCase().replace('template', '');

                var templateFunction = Templates[templateKey];

                if (!templateFunction) {
                    throw 'couldn\'t find pre-compiled template ' + templateKey;
                }

                return function (data) {

                    try {
                        var wrappedTemplate = Handlebars.template.call(Handlebars, templateFunction);
                        return wrappedTemplate(data);
                    }
                    catch (error) {
                        console.error('template render failed for ' + templateKey + ' ' + error);
                        console.error(data);
                        throw error;
                    }
                };
            };
        };
    });


