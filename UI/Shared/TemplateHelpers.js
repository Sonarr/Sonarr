"use strict";

define(['app'], function () {
    Handlebars.registerHelper('partial', function (templateName) {
        //TODO: We should be able to pass in the context, either an object or a property

        var templateFunction = Marionette.TemplateCache.get(templateName);
        return new Handlebars.SafeString(templateFunction(this));
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

    Handlebars.registerHelper("fileSize", function(size) {
        return NzbDrone.Shared.FormatHelpers.FileSizeHelper(size);
    });

    Handlebars.registerHelper("date", function(date) {
        //TODO: show actual date in tooltip
        if (!date) {
            return '';
        }

        var shortDate = Date.create(date).short();
        var formattedDate = NzbDrone.Shared.FormatHelpers.DateHelper(date);
        var result = '<span title="' + shortDate + '">' + formattedDate + '</span>';

        return new Handlebars.SafeString(result);
    });
});