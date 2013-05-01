"use strict";

Marionette.TemplateCache.get = function (templateId) {

    var templateKey = templateId.toLowerCase();

    var templateFunction = window.Templates[templateKey];

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
        }
    };
};