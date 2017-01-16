var Backbone = require('backbone');
var HealthModel = require('./HealthModel');
require('../Mixins/backbone.signalr.mixin');

var Collection = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/health',
    model : HealthModel
});

var collection = new Collection().bindSignalR();
collection.fetch();

module.exports = collection;