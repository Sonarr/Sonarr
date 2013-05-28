'use strict';

define(['app', 'Quality/QualityProfileCollection'], function (app, qualityProfiles) {

    Handlebars.registerHelper('qualityProfileSelection', function () {

        var templateFunction = Marionette.TemplateCache.get('Quality/QualityProfileSelectionTemplate');
        return new Handlebars.SafeString(templateFunction(qualityProfiles.toJSON()));
    });
});
