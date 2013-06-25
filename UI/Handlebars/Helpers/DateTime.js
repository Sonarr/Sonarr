'use strict';
define(
    [
        'handlebars',
        'sugar'
    ], function (Handlebars) {
        Handlebars.registerHelper('ShortDate', function (input) {
            if (!input) {
                return '';
            }

            var date = Date.create(input);
            var result = '<span title="' + date.full() + '">' + date.short() + '</span>';

            return new Handlebars.SafeString(result);
        });
    });
