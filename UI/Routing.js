"use strict";
require(['app', 'Controller', 'RouteBinder'], function (app, controller, routeBinder) {

    NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

        controller: controller,
        appRoutes : {
            ''                           : 'series',
            'series'                     : 'series',
            'series/index'               : 'series',
            'series/add'                 : 'addSeries',
            'series/add/:action(/:query)': 'addSeries',
            'series/details/:query'      : 'seriesDetails',
            'calendar'                   : 'calendar',
            'settings'                   : 'settings',
            'settings/:action(/:query)'  : 'settings',
            'missing'                    : 'missing',
            'history'                    : 'history',
            'logs'                       : 'logs',
            'rss'                        : 'rss',
            ':whatever'                  : 'notFound'
        }
    });

    NzbDrone.addInitializer(function () {

        NzbDrone.Router = new NzbDrone.Router();
        Backbone.history.start({ pushState: true });

        routeBinder.bind();
        NzbDrone.footerRegion.show(new NzbDrone.Shared.Footer.View());
    });
});

