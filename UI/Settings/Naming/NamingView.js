'use strict';
define(['app', 'Settings/Naming/NamingModel'], function () {

    NzbDrone.Settings.Naming.NamingView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Naming/NamingTemplate',

        ui: {
            tooltip: '[class^="help-inline"] i'
        },

        initialize: function () {
            this.model = new NzbDrone.Settings.Naming.NamingModel();
            this.model.fetch();

            NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
        },

        onRender: function () {
            //TODO: Move this to a mixin
            this.ui.tooltip.tooltip({ placement: 'right' });
        },

        saveSettings: function () {
            this.model.save(undefined, NzbDrone.Settings.SyncNotificaiton.callback({
                successMessage: 'Naming Settings saved',
                errorMessage: "Failed to save Naming Settings"
            }));
        }
    });
})
;
