'use strict';
define(
    [
        'templates',
        'handlebars.helpers',
        'Handlebars/Helpers/DateTime',
        'Handlebars/Helpers/Html',
        'Handlebars/Helpers/Numbers',
        'Handlebars/Helpers/Episode',
        'Handlebars/Helpers/Series',
        'Handlebars/Helpers/Quality',
        'Handlebars/Handlebars.Debug'
    ], function (Templates) {
        return function () {
            this.get = function (templateId) {

                var templateKey = templateId.toLowerCase();

                var templateFunction = Templates[templateKey];

                if (!templateFunction) {
                    throw 'couldn\'t find pre-compiled template ' + templateKey;
                }

                return function (data) {

                    try {
                        return templateFunction(data);
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


