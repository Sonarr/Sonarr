'use strict';
define(
    [
        'handlebars',
        'Shared/FormatHelpers'
    ], function (Handlebars, FormatHelpers) {
        Handlebars.registerHelper('Bytes', function (size) {
            return new Handlebars.SafeString(FormatHelpers.Bytes(size));
        });

        Handlebars.registerHelper('Pad2', function (input) {
            return input.pad(2);
        });
    });
