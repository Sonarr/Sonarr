'use strict';
define(
    [
        'sugar'
    ], {
        register: function (handlebars) {
            handlebars.registerHelper('ShortDate', function (input) {
                if (!input) {
                    return '';
                }

                var date = Date.create(input);
                var result = '<span title="' + date.full() + '">' + date.short() + '</span>';

                return new handlebars.SafeString(result);
            });
        }
    });
