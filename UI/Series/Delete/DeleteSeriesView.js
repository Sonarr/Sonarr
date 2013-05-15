'use strict';
define(['app', 'Series/SeriesModel'], function () {

    NzbDrone.Series.Delete.DeleteSeriesView = Backbone.Marionette.ItemView.extend({
        template : 'Series/Delete/DeleteSeriesTemplate',

        events: {
            'click .x-confirm-delete': 'removeSeries'
        },

        ui: {
            deleteFiles: '.x-delete-files'
        },

        removeSeries: function () {

            var deleteFiles = this.ui.deleteFiles.prop('checked');

            this.model.destroy({
                data   : { 'deleteFiles': deleteFiles },
                success: function (model) {
                    model.collection.remove(model);
                }
            });

            NzbDrone.modalRegion.close();

        }
    });
});
