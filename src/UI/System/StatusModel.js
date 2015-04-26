var Backbone = require('backbone');
var ApiData = require('../Shared/ApiData');

var StatusModel = Backbone.Model.extend({
    url : window.NzbDrone.ApiRoot + '/system/status'
});
var instance = new StatusModel(ApiData.get('system/status'));

module.exports = instance;