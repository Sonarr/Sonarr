var PageableCollection = require('backbone.pageable');
var BackupModel = require('./BackupModel');

module.exports = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/system/backup',
    model : BackupModel,
    state : {
        sortKey  : 'time',
        order    : 1,
        pageSize : 100000
    },
    mode  : 'client'
});