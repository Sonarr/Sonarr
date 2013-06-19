'use strict';

define(['app', 'Series/Index/Posters/ItemView', 'Config'], function () {

    NzbDrone.Series.Index.Posters.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Series.Index.Posters.ItemView,
        itemViewContainer       : '#x-series-posters',
        template                : 'Series/Index/Posters/CollectionTemplate'
    });

    return  NzbDrone.Series.Index.Posters.CollectionView;
});
