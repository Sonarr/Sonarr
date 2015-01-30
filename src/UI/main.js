require.config({
    urlArgs: 'v=' + window.NzbDrone.Version
});

if (window.NzbDrone.Production) {
    require.config({
        waitSeconds: 45
    });
}

define(
    [
        'app'
    ], function (app) {
        'use strict';
        app.start();
    });