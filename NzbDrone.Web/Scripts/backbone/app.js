NzbDrone = {};

NzbDrone.Views = {};
NzbDrone.Models = {};
NzbDrone.Collections = {};

NzbDrone.App = new Backbone.Marionette.Application();

// Setup default application views
NzbDrone.App.addInitializer(function () {

    NzbDrone.App.addRegions({
        main: '#main-region'
    });

    var layout = new NzbDrone.Views.AppLayout();

    NzbDrone.App.Layout = layout;
    NzbDrone.App.main.show(layout);

    layout.header.show(new NzbDrone.Views.HeaderView());
});

NzbDrone.App.addInitializer(function () {
    new NzbDrone.AppRouter();
    Backbone.history.start();
});