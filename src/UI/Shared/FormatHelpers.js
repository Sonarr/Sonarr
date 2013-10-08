'use strict';

define(
    [
        'moment',
        'filesize'
    ], function (Moment, Filesize) {

        return {

            bytes: function (sourceSize) {
                var size = Number(sourceSize);
                return Filesize(size, 1, false);
            },

            dateHelper: function (sourceDate) {
                if (!sourceDate) {
                    return '';
                }

                var date = Moment(sourceDate);

                var calendarDate = date.calendar();

                //TODO: It would be nice to not have to hack this...
                var strippedCalendarDate = calendarDate.substring(0, calendarDate.indexOf(' at '));

                if (strippedCalendarDate){
                    return strippedCalendarDate;
                }

                if (date.isAfter(Moment())) {
                    return date.fromNow(true);
                }

                if (date.isBefore(Moment().add('years', -1))) {
                    return date.format('ll');
                }

                return date.fromNow();
            },

            pad: function(n, width, z) {
                z = z || '0';
                n = n + '';
                return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
            },

            number: function (input) {
                if (!input) {
                    return '';
                }

                return input.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            }
        }
    });
