'use strict';

define(['app'], function (app) {

    NzbDrone.Series.Index.EmptySeriesCollectionView = Backbone.Marionette.CompositeView.extend({
        template: 'Series/Index/EmptySeriesIndexTemplate'
    });
});