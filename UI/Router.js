"use strict";
require(
    [
        'marionette',
        'Controller'
    ], function (Marionette, Controller) {

        return Marionette.AppRouter.extend({

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
    });

