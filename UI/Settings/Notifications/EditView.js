"use strict";

define([
    'app',
    'Settings/Notifications/Model'

], function () {

    NzbDrone.Settings.Notifications.EditView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/EditTemplate',

        events: {
            'click .x-save': 'save'
        },

        save: function () {
            this.model.save();

//            window.alert('saving');
//            this.model.save(undefined, this.syncNotification("Notification Settings Saved", "Couldn't Save Notification Settings"));
        },


        syncNotification: function (success, error) {
            return {
                success: function () {
                    window.alert(success);
                },

                error: function () {
                    window.alert(error);
                }
            };
        }
    });
});
