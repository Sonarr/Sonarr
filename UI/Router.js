'use strict';
require(
    [
        'app',
        'backbone',
        'marionette',
        'Controller',
        'Series/SeriesCollection',
        'ProgressMessaging/ProgressMessageCollection',
        'Commands/CommandMessengerCollectionView',
        'Navbar/NavbarView',
        'jQuery/RouteBinder',
        'jquery'
    ], function (App,
                 Backbone,
                 Marionette,
                 Controller,
                 SeriesCollection,
                 ProgressMessageCollection,
                 CommandMessengerCollectionView,
                 NavbarView,
                 RouterBinder,
                 $) {

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
                'rss'                       : 'rss',
                'system'                    : 'system',
                'system/:action'            : 'system',
                'seasonpass'                : 'seasonPass',
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

