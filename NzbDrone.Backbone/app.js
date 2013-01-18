NzbDrone = new Backbone.Marionette.Application();

NzbDrone.MyRouter = Backbone.Marionette.AppRouter.extend({

    // "someMethod" must exist at controller.someMethod
    appRoutes: {
        "add": "AddSeries",
        "add/new": "AddNewSeries",
        "add/existing": "AddExistingSeries",
    }

});

NzbDrone.addInitializer(function (options) {
    new NzbDrone.MyRouter();
    Backbone.history.start();
});