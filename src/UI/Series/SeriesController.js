'use strict';
define(
    [
        'Shared/NzbDroneController',
        'AppLayout',
        'Series/SeriesCollection',
        'Series/Index/SeriesIndexLayout',
        'Series/Details/SeriesDetailsLayout'
    ], function (NzbDroneController, AppLayout, SeriesCollection, SeriesIndexLayout, SeriesDetailsLayout) {

        return NzbDroneController.extend({


            initialize: function () {
                this.route('', this.series);
                this.route('series', this.series);
                this.route('series/:query', this.seriesDetails);
            },


            series: function () {
                this.setTitle('NzbDrone');
                AppLayout.mainRegion.show(new SeriesIndexLayout());
            },

            seriesDetails: function (query) {
                var series = SeriesCollection.where({titleSlug: query});

                if (series.length !== 0) {
                    var targetSeries = series[0];
                    this.setTitle(targetSeries.get('title'));
                    AppLayout.mainRegion.show(new SeriesDetailsLayout({ model: targetSeries }));
                }
                else {
                    this.showNotFound();
                }
            }
        });
    });

