var Backbone = require('backbone');
var HealthModel = require('./HealthModel');
require('../Mixins/backbone.signalr.mixin');

module.exports = (function(){
    var Collection = Backbone.Collection.extend({
        url   : window.NzbDrone.ApiRoot + '/health',
        model : HealthModel
    });
    var collection = new Collection().bindSignalR();
    collection.fetch();
    return collection;
}).call(this);