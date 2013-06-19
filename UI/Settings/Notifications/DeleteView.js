'use strict';
define(['app', 'marionette'], function (App, Marionette) {
    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/DeleteTemplate',

        events: {
            'click .x-confirm-delete': 'removeNotification'
        },

        removeNotification: function () {
            this.model.destroy({
                wait   : true,
                success: function () {
                    App.modalRegion.closeModal();
                }
            });
        }
    });
});
