'use strict';
define(['app', 'marionette'], function (App, Marionette) {
    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/DeleteTemplate',

        events: {
            'click .x-confirm-delete': '_removeIndexer'
        },

        _removeIndexer: function () {
            this.model.destroy({
                wait   : true,
                success: function () {
                    App.vent.trigger(App.Commands.CloseModalCommand);
                }
            });
        }
    });
});
