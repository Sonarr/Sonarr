"use strict";

define([
    'app',
    'Settings/Indexers/Collection'

], function () {

    NzbDrone.Settings.Indexers.ItemView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Indexers/ItemTemplate',
        initialize: function () {
            this.model.set('fields',  [
                { key: 'username', title: 'Username', helpText: 'HALP!', value: 'mark' },
                { key: 'apiKey', title: 'API Key', helpText: 'HALP!', value: 'private' }
            ]);

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
