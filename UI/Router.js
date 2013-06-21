"use strict";
require(
    [
        'app',
        'marionette',
        'Controller',
        'jQuery/RouteBinder'
    ], function (App, Marionette, Controller, RouterBinder) {

        NzbDrone.Router = Marionette.AppRouter.extend({

            controller: Controller,
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

            RouterBinder.bind(NzbDrone.Router);
            // NzbDrone.footerRegion.show(new FooterView());
        });


        return NzbDrone.Router;

    });

