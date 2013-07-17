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

        Handlebars.registerHelper('Day', function (input) {
            if (!input) {
                return '';
            }

            return Date.create(input).format('{dd}');
        });

        Handlebars.registerHelper('Month', function (input) {
            if (!input) {
                return '';
            }

            return Date.create(input).format('{Mon}');
        });

        Handlebars.registerHelper('StartTime', function (input) {
            if (!input) {
                return '';
            }

            var date = Date.create(input);
            if (date.format('{mm}') === '00') {
                return date.format('{h}{tt}');
            }

            return date.format('{h}.{mm}{tt}');
        });
    });
