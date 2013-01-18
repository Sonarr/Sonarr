NzbDrone = new Backbone.Marionette.Application();

NzbDrone.Controller = {

    AddSeries: function () {

        var view = new NzbDrone.AddSeriesView();
        NzbDrone.mainRegion.show(view);
    },

    AddNewSeries: function () {
        alert("AddNewSeries");
    },


    AddExistingSeries: function () {
        alert("AddExistingSeries");
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