var PageableCollection = require('backbone.pageable');
var TaskModel = require('./TaskModel');

module.exports = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/system/task',
    model : TaskModel,
    state : {
        sortKey  : 'name',
        order    : -1,
        pageSize : 100000
    },
    mode  : 'client'
});