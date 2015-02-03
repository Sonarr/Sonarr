var Backbone = require('backbone');
var UpdateModel = require('./UpdateModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/update',
    model : UpdateModel
});