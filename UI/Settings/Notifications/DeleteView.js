'use strict';
define(['app', 'Settings/Notifications/Model'], function () {

    NzbDrone.Settings.Notifications.DeleteView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Notifications/DeleteTemplate',

        events: {
            'click .x-confirm-delete': 'removeNotification'
        },

        removeNotification: function () {
            var self = this;

            this.model.destroy({
                wait   : true,
                success: function (model) {
                    NzbDrone.modalRegion.closeModal();
                }
            });
        }
    });
});
