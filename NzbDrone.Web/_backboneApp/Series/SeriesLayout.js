'use strict;'
define(['app', 'Series/SeriesCollection', 'Series/SeriesCollectionView', 'bootstrap'], function () {


    NzbDrone.Series.SeriesLayout = Backbone.Marionette.Layout.extend({
        template: 'Series/SeriesLayoutTemplate',
        className: "row",
        route: 'Series/index',

        ui: {

        },

        regions: {
            seriesRegion: '#series'
        },

        collection: new NzbDrone.Series.SeriesCollection(),

        initialize: function (options) {

        },

        onRender: function () {
            console.log('binding auto complete');

            this.collection.fetch();

            this.seriesRegion.show(new NzbDrone.Series.SeriesCollectionView({ collection: this.collection }));
        },
    });

});
