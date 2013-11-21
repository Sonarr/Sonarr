require.config({
    urlArgs: 'v=' + window.NzbDrone.Version
});

define(
    [
        'app'
    ], function (app) {
        'use strict';
        app.start();
    });