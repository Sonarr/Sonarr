var Backbone = require('backbone');
var NotificationModel = require('./NotificationModel');

module.exports = Backbone.Collection.extend({
    model : NotificationModel,
    url   : window.NzbDrone.ApiRoot + '/notification'
});