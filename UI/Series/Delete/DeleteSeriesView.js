'use strict';
define(
    [
        'app',
        'marionette'
    ], function (App, Marionette) {

        return  Marionette.ItemView.extend({
            template: 'Series/Delete/DeleteSeriesTemplate',

            events: {
                'click .x-confirm-delete': 'removeSeries'
            },

            ui: {
                deleteFiles: '.x-delete-files'
            },

            removeSeries: function () {
                var self = this;
                var deleteFiles = this.ui.deleteFiles.prop('checked');

                this.model.destroy({
                    data: { 'deleteFiles': deleteFiles },
                    wait: true
                }).done(function () {
                        App.vent.trigger(App.Events.SeriesDeleted, { series: self.model });
                        App.modalRegion.closeModal();
                    });
            }
        });
    });
