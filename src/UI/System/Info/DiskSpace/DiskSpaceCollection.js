var Backbone = require('backbone');
var DiskSpaceModel = require('./DiskSpaceModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/diskspace',
    model : DiskSpaceModel
});