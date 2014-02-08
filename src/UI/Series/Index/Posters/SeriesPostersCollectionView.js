'use strict';

define(
    [
        'marionette',
        'Series/Index/Posters/SeriesPostersItemView'
    ], function (Marionette, PosterItemView) {

        return Marionette.CompositeView.extend({
            itemView         : PosterItemView,
            itemViewContainer: '#x-series-posters',
            template         : 'Series/Index/Posters/SeriesPostersCollectionViewTemplate'
        });
    });
