'use strict';
define(
    [
        'backbone.pageable',
        'System/Backup/BackupModel'
    ], function (PageableCollection, BackupModel) {
        return  PageableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/system/backup',
            model: BackupModel,

            state: {
                sortKey  : 'time',
                order    : 1,
                pageSize : 100000
            },

            mode: 'client'
        });
    });
