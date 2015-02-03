var Backbone = require('backbone');
var RemotePathMappingModel = require('./RemotePathMappingModel');

module.exports = Backbone.Collection.extend({
    model : RemotePathMappingModel,
    url   : window.NzbDrone.ApiRoot + '/remotePathMapping'
});