'use strict';
require(
    [
        'app',
        'marionette',
        'Controller',
        'jQuery/RouteBinder'
    ], function (App, Marionette, Controller, RouterBinder) {

        var Router = Marionette.AppRouter.extend({

            controller: new Controller(),
            appRoutes : {
                ''                          : 'series',
                'series'                    : 'series',
                'addseries'                 : 'addSeries',
                'addseries/:action(/:query)': 'addSeries',
                'series/:query'             : 'seriesDetails',
                'calendar'                  : 'calendar',
                'settings'                  : 'settings',
                'settings/:action(/:query)' : 'settings',
                'missing'                   : 'missing',
                'history'                   : 'history',
                'logs'                      : 'logs',
                'rss'                       : 'rss',
                'system'                    : 'system',
                ':whatever'                 : 'notFound'
            }
        });

        App.addInitializer(function () {

            App.Router = new Router();
            Backbone.history.start({ pushState: true });

            RouterBinder.bind(App.Router);
        });

        return App.Router;

    });

