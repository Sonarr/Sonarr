"use strict";
define(['app'], function () {

    NzbDrone.Commands.Execute = function (name) {
        return $.ajax({
            type: 'POST',
            url : NzbDrone.Constants.ApiRoot + '/command',
            data: JSON.stringify({command: name})
        });
    };
});