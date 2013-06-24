'use strict';
define(
    [
        'app',
        'backgrid',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView'
    ], function (App, Backgrid, EditSeriesView, DeleteSeriesView) {
        return Backgrid.Row.extend({
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

