var Backbone = require('backbone');
var LogFileModel = require('./LogFileModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/log/file/update',
    model : LogFileModel,
    state : {
        sortKey : 'lastWriteTime',
        order   : 1
    }
});