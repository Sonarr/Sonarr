NzbDrone = new Backbone.Marionette.Application();

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
        NzbDrone.mainRegion.show(new NzbDrone.AddSeriesLayout());
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