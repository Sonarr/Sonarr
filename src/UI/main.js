require.config({
    urlArgs: 'v=' + window.NzbDrone.Version
});

if (window.location.hostname.toLowerCase() !== 'localhost') {
    require.config({
        waitSeconds: 30
    });
}

define(
    [
        'app'
    ], function (app) {
        'use strict';
        app.start();
    });