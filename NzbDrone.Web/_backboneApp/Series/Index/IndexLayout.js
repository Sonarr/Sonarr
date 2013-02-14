'use strict;'

/// <reference path="../SeriesCollection.js" />
/// <reference path="SeriesItemView.js" />
/// <reference path="../../JsLibraries/jquery.dataTables.bootstrap.pagination.js" />

NzbDrone.Series.IndexLayout = Backbone.Marionette.Layout.extend({
    template: 'Series/Index/IndexLayoutTemplate',
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
        
        this.seriesRegion.show(new NzbDrone.Series.Index.SeriesCollectionView({ collection: this.collection }));
    },
});