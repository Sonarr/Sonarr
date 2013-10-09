'use strict';
define(
    [
        'vent',
        'marionette'
    ], function (vent, Marionette) {

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
                        vent.trigger(vent.Events.SeriesDeleted, { series: self.model });
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });
            }
        });
    });
