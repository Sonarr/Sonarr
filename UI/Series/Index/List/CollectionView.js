'use strict';

define(['app', 'Series/Index/List/ItemView', 'Config'], function () {

    NzbDrone.Series.Index.List.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Series.Index.List.ItemView,
        itemViewContainer       : '#x-series-list',
        template                : 'Series/Index/List/CollectionTemplate'
    });

    return NzbDrone.Series.Index.List.CollectionView;
});
