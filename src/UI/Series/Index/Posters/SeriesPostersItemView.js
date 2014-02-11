﻿'use strict';

define(
    [
        'vent',
        'marionette'
    ], function (vent, Marionette) {

        return Marionette.ItemView.extend({
            tagName : 'li',
            template: 'Series/Index/Posters/SeriesPostersItemViewTemplate',


            ui: {
                'progressbar': '.progress .bar',
                'controls'   : '.series-controls'
            },

            events: {
                'click .x-edit'              : 'editSeries',
                'click .x-remove'            : 'removeSeries',
                'mouseenter .x-series-poster': 'posterHoverAction',
                'mouseleave .x-series-poster': 'posterHoverAction'
            },


            editSeries: function () {
                vent.trigger(vent.Commands.EditSeriesCommand, {series:this.model});
            },

            removeSeries: function () {
                vent.trigger(vent.Commands.DeleteSeriesCommand, {series:this.model});
            },

            posterHoverAction: function () {
                this.ui.controls.slideToggle();
            }
        });
    });
