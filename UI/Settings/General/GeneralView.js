'use strict';
define(['app', 'Settings/SettingsModel', 'Shared/Messenger'], function () {

    NzbDrone.Settings.General.GeneralView = Backbone.Marionette.ItemView.extend({
            template: 'Settings/General/GeneralTemplate',

            initialize: function () {
                NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
            },

            saveSettings: function () {
                if (!this.model.isSaved) {
                    this.model.save(undefined, NzbDrone.Settings.SyncNotificaiton.callback({
                        successMessage: 'General Settings saved',
                        errorMessage: "Failed to save General Settings"
                    }));
                }
            }
        }
    );
});

