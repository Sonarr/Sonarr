'use strict';
define(
    [
        'vent',
        'marionette'
    ], function (vent, Marionette) {

        return  Marionette.ItemView.extend({
            template: 'Activity/Queue/RemoveFromQueueViewTemplate',

            events: {
                'click .x-confirm-remove' : 'removeItem'
            },

            ui: {
                blacklist : '.x-blacklist',
                indicator : '.x-indicator'
            },

            initialize: function (options) {
                this.templateHelpers = {
                    showBlacklist: options.showBlacklist
                };
            },

            removeItem: function () {
                var blacklist = this.ui.blacklist.prop('checked');

                this.ui.indicator.show();

                this.model.destroy({
                    data: { 'blacklist': blacklist },
                    wait: true
                }).done(function () {
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });
            }
        });
    });
