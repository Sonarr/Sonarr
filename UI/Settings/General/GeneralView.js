'use strict';
define(['app', 'Settings/SettingsModel', 'Shared/Messenger'], function () {

    NzbDrone.Settings.General.GeneralView = Backbone.Marionette.ItemView.extend({
            template: 'Settings/General/GeneralTemplate',

            initialize: function () {

                NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
            },

            saveSettings: function () {
                if (!this.model.isSaved) {
                    this.model.save(undefined, this.syncNotification("Naming Settings Saved", "Couldn't Save Naming Settings"));
                }
            },

            syncNotification: function (success, error) {
                return {
                    success: function () {
                        NzbDrone.Shared.Messenger.show({message: 'General Settings Saved'});
                    },
                    error  : function () {
                        NzbDrone.Shared.Messenger.show({message: "Couldn't Save General Settings", type: 'error'});
                    }
                };
            }
        }
    );
});

