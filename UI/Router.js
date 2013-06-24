﻿'use strict';
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

        App.addInitializer(function () {

            App.Router = new Router();
            Backbone.history.start({ pushState: true });

            RouterBinder.bind(App.Router);
        });


        return App.Router;

    });

