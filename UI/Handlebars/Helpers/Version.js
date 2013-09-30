'use strict';

define(
    [
        'handlebars'
    ], function (Handlebars) {

        Handlebars.registerHelper('currentVersion', function (version) {
            var currentVersion = window.NzbDrone.ServerStatus.version;

            if (currentVersion === version)
            {
                return new Handlebars.SafeString('<i class="icon-ok" title="Installed"></i>');
            }

            return '';
        });
    });
