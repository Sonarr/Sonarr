'use strict';
define(
    [
        'handlebars',
        'moment',
        'Shared/FormatHelpers'
    ], function (Handlebars, Moment, FormatHelpers) {
        Handlebars.registerHelper('ShortDate', function (input) {
            if (!input) {
                return '';
            }

            var date = Moment(input);
            var result = '<span title="' + date.format('LLLL') + '">' + date.format('LL') + '</span>';

            return new Handlebars.SafeString(result);
        });

        Handlebars.registerHelper('NextAiring', function (input) {
            if (!input) {
                return '';
            }

            var date = Moment(input);
            var result = '<span title="' + date.format('LLLL') + '">' + FormatHelpers.dateHelper(input) + '</span>';

            return new Handlebars.SafeString(result);
        });

        Handlebars.registerHelper('Day', function (input) {
            if (!input) {
                return '';
            }

            return Moment(input).format('DD');
        });

        Handlebars.registerHelper('Month', function (input) {
            if (!input) {
                return '';
            }

            return Moment(input).format('MMM');
        });

        Handlebars.registerHelper('StartTime', function (input) {
            if (!input) {
                return '';
            }

            var date = Moment(input);
            if (date.format('mm') === '00') {
                return date.format('ha');
            }

            return date.format('h.mma');
        });
    });
