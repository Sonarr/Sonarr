'use strict';

define(['app', 'Series/SeriesModel'], function () {

    NzbDrone.Series.Delete.DeleteSeriesView = Backbone.Marionette.ItemView.extend({
        template:'Series/Delete/DeleteSeriesTemplate',
        tagName:'div',
        className:"modal",

        events:{
            'click .x-confirm-delete':'removeSeries'
        },

        ui:{
            deleteFiles:'.x-delete-files'
        },

        onRender:function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        },

        removeSeries:function () {

            var deleteFiles = this.ui.deleteFiles.prop('checked');

            this.model.destroy({
                data:{ 'deleteFiles':deleteFiles },
                success:function (model) {
                    model.collection.remove(model);
                }
            });

            NzbDrone.modalRegion.close();

        }
    });
});