'use strict';
require(
    [
        'app',
        'marionette',
        'Controller',
        'Series/SeriesCollection',
        'jQuery/RouteBinder'
    ], function (App, Marionette, Controller, SeriesCollection, RouterBinder) {

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
                'logs/:action'              : 'logs',
                'rss'                       : 'rss',
                'system'                    : 'system',
                'seasonpass'                : 'seasonPass',
                ':whatever'                 : 'notFound'
            }
        });

        App.addInitializer(function () {

            App.Router = new Router();

            SeriesCollection.fetch()
                .done(function(){
                    Backbone.history.start({ pushState: true });
                    RouterBinder.bind(App.Router);
                })
        });

        return App.Router;

    });

