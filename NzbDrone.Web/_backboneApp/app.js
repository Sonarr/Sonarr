/// <reference path="JsLibraries/jquery.js" />
/// <reference path="JsLibraries/underscore.js" />
/// <reference path="JsLibraries/sugar.js" />
/// <reference path="JsLibraries/backbone.js" />
/// <reference path="JsLibraries/backbone.modelbinder.js" />
/// <reference path="JsLibraries/backbone.mutators.js" />
/// <reference path="JsLibraries/backbone.shortcuts.js" />
/// <reference path="JsLibraries/backbone.marionette.js" />
/// <reference path="JsLibraries/backbone.marionette.extend.js" />
/// <reference path="JsLibraries/backbone.modelbinder.js" />
/// <reference path="JsLibraries/bootstrap.js" />
/// <reference path="errorHandler.js" />

if (typeof console == "undefined") {
    window.console = { log: function () { } };
}

NzbDrone = new Backbone.Marionette.Application();
NzbDrone.AddSeries = NzbDrone.module("AddSeries");

NzbDrone.ModelBinder = new Backbone.ModelBinder();


NzbDrone.Constants = {
    ApiRoot: '/api/'
};

NzbDrone.Events = {
    DisplayInMainRegion: "DisplayInMainRegion",
};


NzbDrone.Routes = {
    Series: {
        Add: 'series/add',
    },
};


NzbDrone.Controller = Backbone.Marionette.Controller.extend({

    addSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout());
    },


    notFound: function () {
        alert('route not found');
    },
});


NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

    controller: new NzbDrone.Controller(),
    // "someMethod" must exist at controller.someMethod
    appRoutes: {
        "series/add": "addSeries",
        ":whatever": "notFound"

    }

});

NzbDrone.addInitializer(function (options) {

    console.log("starting application");


    NzbDrone.addRegions({
        mainRegion: "#main-region",
    });

    NzbDrone.Router = new NzbDrone.Router();
    Backbone.history.start();


});