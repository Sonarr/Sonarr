var Backbone = require('backbone');
var CommandModel = require('./CommandModel');
require('../Mixins/backbone.signalr.mixin');

module.exports = (function(){
    var CommandCollection = Backbone.Collection.extend({
        url         : window.NzbDrone.ApiRoot + '/command',
        model       : CommandModel,
        findCommand : function(command){
            return this.find(function(model){
                return model.isSameCommand(command);
            });
        }
    });
    var collection = new CommandCollection().bindSignalR();
    collection.fetch();
    return collection;
}).call(this);