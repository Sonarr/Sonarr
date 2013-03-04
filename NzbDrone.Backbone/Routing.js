require(['app','Controller'], function (app, controller) {

    NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

        controller: controller,
        appRoutes: {
            '': 'series',
            'series': 'series',
            'series/index': 'series',
            'series/add': 'addSeries',
            'series/add/:action(/:query)': 'addSeries',
            'series/details/:query': 'seriesDetails',
            'upcoming': 'upcoming',
            'upcoming/index': 'upcoming',
            'calendar': 'calendar',
            'settings': 'settings',
            'settings/:action(/:query)': 'settings',
            ':whatever': 'notFound'
        }
    });

    NzbDrone.addInitializer(function () {

        NzbDrone.Router = new NzbDrone.Router();
        Backbone.history.start({ pushState: true });

    });
});

