"use strict";
define(['app'], function () {

    NzbDrone.Commands.Execute = function (name, properties) {
        var data = { command: name };

        if (properties !== undefined) {
            $.extend(data, properties);
        }

        return $.ajax({
            type: 'POST',
            url : NzbDrone.Constants.ApiRoot + '/command',
            data: JSON.stringify(data)
        });
    };
});