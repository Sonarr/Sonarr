'use strict';
define(
    [
        'marionette',
        'SeasonPass/SeriesLayout'
    ], function (Marionette, SeriesLayout) {
        return Marionette.CollectionView.extend({
            itemView: SeriesLayout
        });
    });
