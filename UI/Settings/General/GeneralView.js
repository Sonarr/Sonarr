'use strict';
define(['app', 'Settings/SettingsModel', 'Shared/Messenger'], function () {

    NzbDrone.Settings.General.GeneralView = Backbone.Marionette.ItemView.extend({
            template: 'Settings/General/GeneralTemplate',

            initialize: function () {
                NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
            },

            saveSettings: function () {
                if (!this.model.isSaved) {
                    this.model.save(undefined, this.syncNotification("General Settings Saved", "Couldn't Save General Settings"));
                }
            },

            syncNotification: function (success, error) {
                return {
                    success: function () {
                        NzbDrone.Shared.Messenger.show({message: success});
                    },
                    error  : function () {
                        NzbDrone.Shared.Messenger.show({message: error, type: 'error'});
                    }
                };
            }
        }
    );
});

