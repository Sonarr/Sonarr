require(['app','Shared/NotificationView', 'AddSeries/AddSeriesLayout','Series/SeriesLayout'], function () {

    NzbDrone.Controller = Backbone.Marionette.Controller.extend({

        addSeries: function (action, query) {
            NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout(this, action, query));
        },

        series: function (action, query) {
            NzbDrone.mainRegion.show(new NzbDrone.Series.SeriesLayout(this, action, query));
        },

        notFound: function () {
            alert('route not found');
        }
    });

    NzbDrone.Router = Backbone.Marionette.AppRouter.extend({

        controller: new NzbDrone.Controller(),
        appRoutes: {
            'series': 'series',
            'series/index': 'series',
            'series/add': 'addSeries',
            'series/add/:action(/:query)': 'addSeries',
            ':whatever': 'notFound'
        }
    });


    NzbDrone.addInitializer(function () {

        NzbDrone.Router = new NzbDrone.Router();
        Backbone.history.start();

    });
});

