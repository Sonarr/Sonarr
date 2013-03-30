"use strict";

Marionette.TemplateCache.get = function (templateId) {

    var templateKey = templateId.toLowerCase();

    var templateFunction = window.Templates[templateKey.toLowerCase()];

    if (!templateFunction) {
        throw 'couldn\'t find pre-compiled template ' + templateKey;
    }

    return function (data) {

        try {
            //console.log('rendering template ' + templateKey);
            return templateFunction(data);
        }
        catch (error) {
            console.error('template render failed for ' + templateKey + ' ' + error.message);
            console.error(data);
        }
    };
};