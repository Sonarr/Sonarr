'use strict';

define(
    [
        'vent',
        'marionette',
        'Series/Index/SeriesIndexItemView'
    ], function (vent, Marionette, SeriesIndexItemView) {
        return SeriesIndexItemView.extend({
            template: 'Series/Index/Overview/SeriesOverviewItemViewTemplate',
        });
    });
