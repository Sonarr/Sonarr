'use strict';

define(['app', 'AddSeries/RootFolders/RootFolderCollection','handlebars'], function (app, rootFolders, Handlebars) {

    Handlebars.registerHelper('rootFolderSelection', function () {
        //TODO: We should be able to pass in the context, either an object or a property

        var templateFunction = Marionette.TemplateCache.get('AddSeries/RootFolders/RootFolderSelectionTemplate');
        return new Handlebars.SafeString(templateFunction(rootFolders.toJSON()));
    });
});
