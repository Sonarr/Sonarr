'use strict';
define(
    [
        'marionette',
        'Series/SeriesCollection',
        'Series/SeasonCollection',
        'SeasonPass/SeriesCollectionView',
        'Shared/LoadingView'
    ], function (Marionette,
                 SeriesCollection,
                 SeasonCollection,
                 SeriesCollectionView,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'SeasonPass/LayoutTemplate',

            regions: {
                series: '#x-series'
            },

            onShow: function () {
                var self = this;

                this.series.show(new LoadingView());

                this.seriesCollection = SeriesCollection;
                this.seasonCollection = new SeasonCollection();

                var promise = this.seasonCollection.fetch();

                promise.done(function () {
                    self.series.show(new SeriesCollectionView({
                        collection: self.seriesCollection,
                        seasonCollection: self.seasonCollection
                    }));
                });
            }
        });
    });
