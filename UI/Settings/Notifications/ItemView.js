"use strict";

define([
    'app',
    'Settings/Notifications/Collection'

], function () {

    NzbDrone.Settings.Notifications.ItemView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/ItemTemplate',
        initialize: function () {
            NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
        },

        saveSettings: function () {

            //this.model.save(undefined, this.syncNotification("Naming Settings Saved", "Couldn't Save Naming Settings"));
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
