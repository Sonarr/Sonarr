var moment = require('moment');
var filesize = require('filesize');
var UiSettings = require('./UiSettingsModel');

module.exports = {
    bytes : function(sourceSize) {
        var size = Number(sourceSize);

        if (isNaN(size)) {
            return '';
        }

        return filesize(size, {
            base  : 2,
            round : 1
        });
    },

    relativeDate : function(sourceDate) {
        if (!sourceDate) {
            return '';
        }

        var date = moment(sourceDate);
        var calendarDate = date.calendar();

        //TODO: It would be nice to not have to hack this...
        var strippedCalendarDate = calendarDate.substring(0, calendarDate.indexOf(' at '));

        if (strippedCalendarDate) {
            return strippedCalendarDate;
        }

        if (date.isAfter(moment())) {
            return 'in ' + date.fromNow(true);
        }

        if (date.isBefore(moment().add('years', -1))) {
            return date.format(UiSettings.get('shortDateFormat'));
        }

        return date.fromNow();
    },

    pad : function(n, width, z) {
        z = z || '0';
        n = n + '';
        return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
    },

    number : function(input) {
        if (!input) {
            return '0';
        }

        return input.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    },

    plural : function(input, unit) {
        if (input === 1) {
            return unit;
        }

        return unit + 's';
    }
};