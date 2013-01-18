NzbDrone = new Backbone.Marionette.Application();

NzbDrone.Controller = {

    AddSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddSeriesView());
    },

    AddNewSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddNewSeriesView());
    },
    
    AddExistingSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddExistingSeriesView());
    }
};


NzbDrone.MyRouter = Backbone.Marionette.AppRouter.extend({

    controller: NzbDrone.Controller,
    // "someMethod" must exist at controller.someMethod
    appRoutes: {
        "add": "AddSeries",
        "add/new": "AddNewSeries",
        "add/existing": "AddExistingSeries",
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