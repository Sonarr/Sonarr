/// <reference path="JsLibraries/jquery.js" />
/// <reference path="JsLibraries/underscore.js" />
/// <reference path="JsLibraries/sugar.js" />
/// <reference path="JsLibraries/backbone.js" />
/// <reference path="JsLibraries/handlebars.js" />
/// <reference path="JsLibraries/backbone.modelbinder.js" />
/// <reference path="JsLibraries/backbone.mutators.js" />
/// <reference path="JsLibraries/backbone.shortcuts.js" />
/// <reference path="JsLibraries/backbone.marionette.js" />
/// <reference path="JsLibraries/backbone.marionette.extend.js" />
/// <reference path="JsLibraries/backbone.marionette.viewswapper.js" />
/// <reference path="JsLibraries/backbone.modelbinder.js" />
/// <reference path="JsLibraries/bootstrap.js" />

if (typeof console === undefined) {
    window.console = { log: function () { } };
}

NzbDrone = new Backbone.Marionette.Application();
NzbDrone.Series = {};
NzbDrone.Series.Index = {};
NzbDrone.AddSeries = {};
NzbDrone.AddSeries.New = {};
NzbDrone.AddSeries.Existing = {};
NzbDrone.AddSeries.RootFolders = {};
NzbDrone.Quality = {};
NzbDrone.Shared = {};

/*
_.templateSettings = {
    interpolate: /\{\{([\s\S]+?)\}\}/g
};
*/

NzbDrone.ModelBinder = new Backbone.ModelBinder();

NzbDrone.Constants = {
    ApiRoot: '/api'
};

NzbDrone.Events = {
    DisplayInMainRegion: 'DisplayInMainRegion'
};

NzbDrone.Controller = Backbone.Marionette.Controller.extend({

    addSeries: function (action, query) {
        NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout(this, action, query));
    },

    series: function (action, query) {
        NzbDrone.mainRegion.show(new NzbDrone.Series.IndexLayout(this, action, query));
    },

    notFound: function () {
        alert('route not found');
    }
});

NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

    controller: new NzbDrone.Controller(),
    // "someMethod" must exist at controller.someMethod
    appRoutes: {
        'series/index': 'series',
        'series/add': 'addSeries',
        'series/add/:action(/:query)': 'addSeries',
        ':whatever': 'notFound'
    }
});

NzbDrone.addInitializer(function (options) {

    console.log('starting application');

    NzbDrone.addRegions({
        mainRegion: '#main-region',
        notificationRegion: '#notification-region'
    });

    NzbDrone.Router = new NzbDrone.Router();
    Backbone.history.start();
});