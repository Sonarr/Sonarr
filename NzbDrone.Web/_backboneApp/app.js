/// <reference path="JsLibraries/jquery-1.8.2.js" />
/// <reference path="JsLibraries/underscore.js" />
/// <reference path="JsLibraries/backbone.js" />
/// <reference path="JsLibraries/backbone.marionette.js" />
/// <reference path="JsLibraries/backbone.marionette.extend.js" />
/// <reference path="JsLibraries/bootstrap.js" />



NzbDrone = new Backbone.Marionette.Application();
NzbDrone.AddSeries = NzbDrone.module("AddSeries");

NzbDrone.Constants = {


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
        NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddNewSeriesView());
    },


    notFound: function () {
        alert('route not found');
    },
});


NzbDrone.MyRouter = Backbone.Marionette.AppRouter.extend({

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

    NzbDrone.Router = new NzbDrone.MyRouter();
    Backbone.history.start();


});