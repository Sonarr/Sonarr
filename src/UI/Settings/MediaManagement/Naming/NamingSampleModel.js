var Backbone = require('backbone');

module.exports = Backbone.Model.extend({url : window.NzbDrone.ApiRoot + '/config/naming/samples'});