'use strict';

define(
    [
        'app',
        'marionette',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView'

    ], function (App, Marionette, EditSeriesView, DeleteSeriesView) {
        return Marionette.ItemView.extend({
            template: 'Series/Index/List/ItemTemplate',

            ui: {
                'progressbar': '.progress .bar'
            },

            events: {
                'click .x-edit'  : 'editSeries',
                'click .x-remove': 'removeSeries'
            },

            editSeries: function () {
                var view = new EditSeriesView({ model: this.model});
                App.modalRegion.show(view);
            },

            removeSeries: function () {
                var view = new DeleteSeriesView({ model: this.model });
                App.modalRegion.show(view);
            }
        });
    });
