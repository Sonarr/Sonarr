"use strict";

Marionette.TemplateCache.get = function (templateId) {
    var templateFunction = window.Templates[templateId];

    if (!templateFunction) {
        console.error('couldn\'t find pre-compiled template ' + templateId);
    }

    var templateName = templateId;

    return function (data) {

        try {
            console.log('rendering template ' + templateName);
            return templateFunction(data);
        }
        catch (error) {
            console.error('template render failed for ' + templateName + ' ' + error.message);
            console.error(data);
        }
    };
};