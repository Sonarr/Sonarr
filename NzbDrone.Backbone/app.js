NzbDrone = new Backbone.Marionette.Application();

NzbDrone.Constants = {


};

NzbDrone.Events = {
    DisplayInMainRegion: "DisplayInMainRegion",
};


NzbDrone.Routes = {
    Series: {
        Add: 'series/add',
        AddNew: 'series/addnew',
        AddExisting: 'series/addExisting',
        AddExistingSingle: 'series/addExisting/single',
        AddExistingMultiple: 'series/addExisting/multiple',
    },
};


NzbDrone.Controller = Backbone.Marionette.Controller.extend({

    AddSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddSeriesView());
    },

    AddNewSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddNewSeriesView());
    },

    AddExistingSeries: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddExistingSeriesView());
    },
    
    AddExistingSeriesSingle: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddExistingSeriesSingleView());
    },
    
    AddExistingSeriesMultiple: function () {
        NzbDrone.mainRegion.show(new NzbDrone.AddExistingSeriesMultipleView());
    },
    

});


NzbDrone.MyRouter = Backbone.Marionette.AppRouter.extend({

    controller: new NzbDrone.Controller(),
    // "someMethod" must exist at controller.someMethod
    appRoutes: {
        "series/add": "AddSeries",
        "series/addnew": "AddNewSeries",
        "series/addExisting": "AddExistingSeries",
        "series/addExisting/single": "AddExistingSeriesSingle",
        "series/addExisting/multiple": "AddExistingSeriesMultiple",
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