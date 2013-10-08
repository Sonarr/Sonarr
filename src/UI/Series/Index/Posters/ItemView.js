﻿'use strict';

define(
    [
        'app',
        'marionette'
    ], function (App, Marionette) {

        return Marionette.ItemView.extend({
            tagName : 'li',
            template: 'Series/Index/Posters/ItemTemplate',


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
                App.vent.trigger(App.Commands.EditSeriesCommand, {series:this.model});
            },

            removeSeries: function () {
                App.vent.trigger(App.Commands.DeleteSeriesCommand, {series:this.model});
            },

            posterHoverAction: function () {
                this.ui.controls.slideToggle();
            }
        });
    });
