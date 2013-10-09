'use strict';
define(
    [
        'marionette',
        'Controller'
    ], function (Marionette, Controller) {

        return Marionette.AppRouter.extend({

            controller: new Controller(),
            appRoutes : {
                'addseries'                 : 'addSeries',
                'addseries/:action(/:query)': 'addSeries',
                'calendar'                  : 'calendar',
                'settings'                  : 'settings',
                'settings/:action(/:query)' : 'settings',
                'missing'                   : 'missing',
                'history'                   : 'history',
                'history/:action'           : 'history',
                'rss'                       : 'rss',
                'system'                    : 'system',
                'system/:action'            : 'system',
                'seasonpass'                : 'seasonPass',
                ':whatever'                 : 'showNotFound'
            }
        });
    });

