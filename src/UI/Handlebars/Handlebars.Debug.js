'use strict';
define(
    [
        'handlebars'
    ], function (Handlebars) {
        Handlebars.registerHelper("debug", function () {
            console.group('Handlebar context');
            console.log(this);
            console.groupEnd();
        });
    });
