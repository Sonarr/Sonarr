require(['app','Controller'], function (app, controller) {

    NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

        controller: controller,
        appRoutes: {
            '': 'series',
            'series': 'series',
            'series/index': 'series',
            'series/add': 'addSeries',
            'series/add/:action(/:query)': 'addSeries',
            ':whatever': 'notFound'
        }
    });

    NzbDrone.addInitializer(function () {

        NzbDrone.Router = new NzbDrone.Router();
        Backbone.history.start({ pushState: true });

    });
});

