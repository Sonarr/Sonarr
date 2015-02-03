var vent = require('../vent');
var DeepModel = require('backbone.deepmodel');
var AsChangeTrackingModel = require('../Mixins/AsChangeTrackingModel');
var Messenger = require('../Shared/Messenger');

module.exports = (function(){
    var model = DeepModel.DeepModel.extend({
        initialize     : function(){
            this.listenTo(vent, vent.Commands.SaveSettings, this.saveSettings);
            this.listenTo(this, 'destroy', this._stopListening);
        },
        saveSettings   : function(){
            if(!this.isSaved) {
                var savePromise = this.save();
                Messenger.monitor({
                    promise        : savePromise,
                    successMessage : this.successMessage,
                    errorMessage   : this.errorMessage
                });
                return savePromise;
            }
            return undefined;
        },
        _stopListening : function(){
            this.stopListening(vent, vent.Commands.SaveSettings);
        }
    });
    return AsChangeTrackingModel.call(model);
}).call(this);