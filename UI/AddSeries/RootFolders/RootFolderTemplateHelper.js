'use strict';

define(
    [
        'AddSeries/RootFolders/Collection',
        'handlebars'
    ], function (rootFolders, Handlebars) {

        Handlebars.registerHelper('rootFolderSelection', function () {
            var templateFunction = Marionette.TemplateCache.get('AddSeries/RootFolders/RootFolderSelectionTemplate');
            return new Handlebars.SafeString(templateFunction(rootFolders.toJSON()));
        });
    });
