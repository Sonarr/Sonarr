﻿'use strict';

define(
    [
        'vent',
        'marionette',
        'Series/Index/SeriesIndexItemView'
    ], function (vent, Marionette, SeriesIndexItemView) {

        return SeriesIndexItemView.extend({
            tagName : 'li',
            template: 'Series/Index/Posters/SeriesPostersItemViewTemplate',

            initialize: function () {
                this.events['mouseenter .x-series-poster'] = 'posterHoverAction';
                this.events['mouseleave .x-series-poster'] = 'posterHoverAction';

                this.ui.controls = '.x-series-controls';
                this.ui.title = '.x-title';
            },

            posterHoverAction: function () {
                this.ui.controls.slideToggle();
                this.ui.title.slideToggle();
            }
        });
    });
