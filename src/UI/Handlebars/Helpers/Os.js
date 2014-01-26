'use strict';
define(
    [
        'handlebars'
    ], function (Handlebars) {
        Handlebars.registerHelper('LinuxOnly', function () {
            return new Handlebars.SafeString('<i class="icon-linux" title="Linux Only"></i>');
        });
    });
