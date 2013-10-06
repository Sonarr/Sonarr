'use strict';

define(
    [
        'handlebars',
        'System/StatusModel'
    ], function (Handlebars, StatusModel) {

        Handlebars.registerHelper('if_windows', function(options) {
            if (StatusModel.get('isWindows'))
            {
                return options.fn(this);
            }

            return options.inverse(this);
        });

        Handlebars.registerHelper('if_linux', function(options) {
            if (StatusModel.get('isLinux'))
            {
                return options.fn(this);
        }

            return options.inverse(this);
        });
    });
