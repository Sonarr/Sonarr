'use strict';
define(
    [
        'vent',
        'marionette'
    ], function (vent, Marionette) {
        return Marionette.ItemView.extend({
            template: 'Settings/Notifications/DeleteTemplate',

            events: {
                'click .x-confirm-delete': '_removeNotification'
            },

            _removeNotification: function () {
                this.model.destroy({
                    wait   : true,
                    success: function () {
                        vent.trigger(vent.Commands.CloseModalCommand);
                    }
                });
            }
        });
    });
