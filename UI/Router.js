'use strict';
require(
    [
        'app',
        'marionette',
        'Controller',
        'Series/SeriesCollection',
        'ProgressMessaging/ProgressMessageCollection',
        'Commands/CommandMessengerCollectionView',
        'Navbar/NavbarView',
        'jQuery/RouteBinder',
        'jquery'
    ], function (App, Marionette, Controller, SeriesCollection, ProgressMessageCollection, CommandMessengerCollectionView, NavbarView, RouterBinder, $) {

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
                'update'                    : 'update',
                ':whatever'                 : 'notFound'
            }
        });

        App.addInitializer(function () {

            App.Router = new Router();

            SeriesCollection.fetch().done(function () {
                Backbone.history.start({ pushState: true });
                RouterBinder.bind(App.Router);
                App.navbarRegion.show(new NavbarView());
                $('body').addClass('started');
            });
        });

        return App.Router;

    });

