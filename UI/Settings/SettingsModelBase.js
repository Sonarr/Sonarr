"use strict";
define(['app',
    'backbone.deepmodel',
    'Mixins/AsChangeTrackingModel',
    'Shared/Messenger'], function (App, DeepModel, AsChangeTrackingModel, Messenger) {
    var model = DeepModel.DeepModel.extend({

        initialize: function () {

           // App.vent.on(App.Commands.SaveSettings, this.saveSettings, this);
            this.listenTo(App.vent, App.Commands.SaveSettings, this.saveSettings);
        },

        saveSettings: function () {

            if (!this.isSaved) {

                var savePromise = this.save();

                Messenger.monitor(
                    {
                        promise       : savePromise,
                        successMessage: this.successMessage,
                        errorMessage  : this.errorMessage
                    });

                return savePromise;
            }

            return undefined;
        }

    });

    return AsChangeTrackingModel.call(model);
});
