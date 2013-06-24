﻿'use strict';

define(
    [
        'app',
        'marionette',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView'

    ], function (App, Marionette, EditSeriesView, DeleteSeriesView) {

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
                var view = new EditSeriesView({ model: this.model});
                App.modalRegion.show(view);
            },

            removeSeries: function () {
                var view = new DeleteSeriesView({ model: this.model });
                App.modalRegion.show(view);
            },

            posterHoverAction: function () {
                this.ui.controls.slideToggle();
            }
        });
    });
