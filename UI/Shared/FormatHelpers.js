'use strict';

define(
    [
        'moment',
        'filesize'
    ], function (Moment, Filesize) {

        return {

            Bytes: function (sourceSize) {
                var size = Number(sourceSize);
                return Filesize(size, 1, false);
            },

            DateHelper: function (sourceDate) {
                if (!sourceDate) {
                    return '';
                }

                var date = Moment(sourceDate);

                if (date.isAfter(Moment().add('days', 6))) {
                    return date.fromNow(true);
                }

                //TODO: It would be nice to not have to hack this...
                var calendarDate = date.calendar();
                return calendarDate.substring(0, calendarDate.indexOf(' at '));
            },

            pad: function(n, width, z) {
                z = z || '0';
                n = n + '';
                return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
            }
        }
    });
