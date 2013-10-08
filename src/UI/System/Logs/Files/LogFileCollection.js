﻿'use strict';

define(['System/Logs/Files/LogFileModel' ],
function (LogFileModel) {
    return Backbone.Collection.extend({
        url  : window.NzbDrone.ApiRoot + '/log/files',
        model: LogFileModel,

        state: {
            sortKey : 'lastWriteTime',
            order   : 1
        }
    });
});
