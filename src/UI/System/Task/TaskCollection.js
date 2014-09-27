'use strict';
define(
    [
        'backbone.pageable',
        'System/Task/TaskModel'
    ], function (PageableCollection, TaskModel) {
        return  PageableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/system/task',
            model: TaskModel,

            state: {
                sortKey  : 'name',
                order    : -1,
                pageSize : 100000
            },

            mode: 'client'
        });
    });
