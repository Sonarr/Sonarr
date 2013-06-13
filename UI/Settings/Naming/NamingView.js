'use strict';
define(['app',
        'Settings/Naming/NamingModel',
        'Settings/SyncNotification'], function () {

    NzbDrone.Settings.Naming.NamingView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Naming/NamingTemplate',

        initialize: function () {
            this.model = new NzbDrone.Settings.Naming.NamingModel();
            this.model.fetch();

            NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
        },

        saveSettings: function () {
            this.model.saveIfChanged(undefined, NzbDrone.Settings.SyncNotificaiton.callback({
                successMessage: 'Naming Settings saved',
                errorMessage: "Failed to save Naming Settings"
            }));
        }
    });
})
;
