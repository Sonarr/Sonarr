'use strict';
define(['app'], function () {
    NzbDrone.Series.Index.EmptySeriesCollectionView = Backbone.Marionette.CompositeView.extend({
        template: 'Series/Index/EmptySeriesIndexTemplate'
    });

    return   NzbDrone.Series.Index.EmptySeriesCollectionView;
});
