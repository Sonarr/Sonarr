'use strict';

define(
    [
        'backbone',
        'System/Logs/Updates/LogFileModel'
    ], function (Backbone, LogFileModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/log/file/update',
            model: LogFileModel,

            state: {
                sortKey: 'lastWriteTime',
                order  : 1
            }
        });
    });
