'use strict';

define(
    [
        'handlebars'
    ], function (Handlebars) {

        Handlebars.registerHelper('currentVersion', function (version, latest) {
            var currentVersion = window.NzbDrone.ServerStatus.version;

            if (currentVersion === version)
            {
                return new Handlebars.SafeString('<i class="icon-ok" title="Installed"></i>');
            }

            if (latest) {
                return new Handlebars.SafeString('<span class="label label-inverse install-update x-install-update">Install</span>');
            }

            return '';
        });
    });
