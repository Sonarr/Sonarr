'use strict;'
/// <reference path="../../app.js" />
/// <reference path="../SeriesCollection.js" />
/// <reference path="SeriesItemView.js" />

NzbDrone.Series.IndexLayout = Backbone.Marionette.Layout.extend({
    template: 'Series/Index/IndexLayoutTemplate',
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
        
        this.seriesRegion.show(new NzbDrone.Series.Index.SeriesCollectionView({ collection: this.collection }));
    },
});