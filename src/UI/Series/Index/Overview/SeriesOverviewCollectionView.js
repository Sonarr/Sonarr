'use strict';

define(
    [
        'marionette',
        'Series/Index/Overview/SeriesOverviewItemView'
    ], function (Marionette, ListItemView) {

        return Marionette.CompositeView.extend({
            itemView         : ListItemView,
            itemViewContainer: '#x-series-list',
            template         : 'Series/Index/Overview/SeriesOverviewCollectionViewTemplate'
        });
    });
