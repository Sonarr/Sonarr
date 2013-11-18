'use strict';
define(function () {
    window.onbeforeunload = function () {
        window.NzbDrone.unloading = true;
    };
});
