'use strict';
define(
    [
        'vent',
        'backbone.deepmodel',
        'Mixins/AsChangeTrackingModel',
        'Shared/Messenger'
    ], function (vent, DeepModel, AsChangeTrackingModel, Messenger) {
        var model = DeepModel.DeepModel.extend({

            initialize: function () {
                this.listenTo(vent, vent.Commands.SaveSettings, this.saveSettings);
                this.listenTo(this, 'destroy', this._stopListening);
            },

            saveSettings: function () {

                if (!this.isSaved) {

                    var savePromise = this.save();

                    Messenger.monitor({
                            promise       : savePromise,
                            successMessage: this.successMessage,
                            errorMessage  : this.errorMessage
                        });

                    return savePromise;
                }

                return undefined;
            },

            _stopListening: function () {
                this.stopListening(vent, vent.Commands.SaveSettings);
            }
        });

        return AsChangeTrackingModel.call(model);
    });
