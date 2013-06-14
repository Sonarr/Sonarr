'use strict';

define(['app', 'Quality/QualityProfileCollection','handlebars'], function (app, qualityProfiles,Handlebars) {

    Handlebars.registerHelper('qualityProfileSelection', function () {

        var templateFunction = Marionette.TemplateCache.get('Quality/QualityProfileSelectionTemplate');
        return new Handlebars.SafeString(templateFunction(qualityProfiles.toJSON()));
    });
});
