'use strict';
define(['app',
    'marionette',
    'Settings/Naming/NamingModel',
    'Settings/SyncNotification',
    'Mixins/AsModelBoundView'], function (App, Marionette, NamingModel, SyncNotification, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Naming/NamingTemplate',

        initialize: function () {
            this.model = new NamingModel();
            this.model.fetch();

            this.listenTo(App.vent, App.Commands.SaveSettings, this.saveSettings);

        },

        saveSettings: function () {
            this.model.saveIfChanged(undefined, SyncNotification.callback({
                successMessage: 'Naming Settings saved',
                errorMessage  : "Failed to save Naming Settings"
            }));
        }
    });

    return AsModelBoundView.call(view);
});
