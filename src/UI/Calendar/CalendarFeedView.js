var Marionette = require('marionette');
var StatusModel = require('../System/StatusModel');
require('../Mixins/CopyToClipboard');

module.exports = Marionette.Layout.extend({
    template : 'Calendar/CalendarFeedViewTemplate',

    ui : {
        includeUnmonitored : '.x-includeUnmonitored',
        premiersOnly       : '.x-premiersOnly',
        icalUrl            : '.x-ical-url',
        icalCopy           : '.x-ical-copy',
        icalWebCal         : '.x-ical-webcal'
    },

    events : {
        'click .x-includeUnmonitored' : '_updateUrl',
        'click .x-premiersOnly'       : '_updateUrl'
    },

    onShow : function() {
        this._updateUrl();
        this.ui.icalCopy.copyToClipboard(this.ui.icalUrl);
    },

    _updateUrl : function() {
        var icalUrl = window.location.host + StatusModel.get('urlBase') + '/feed/calendar/NzbDrone.ics?';

        if (this.ui.includeUnmonitored.prop('checked')) {
            icalUrl += 'unmonitored=true&';
        }

        if (this.ui.premiersOnly.prop('checked')) {
            icalUrl += 'premiersOnly=true&';
        }

        icalUrl += 'apikey=' + window.NzbDrone.ApiKey;

        var icalHttpUrl = window.location.protocol + '//' + icalUrl;
        var icalWebCalUrl = 'webcal://' + icalUrl;

        this.ui.icalUrl.attr('value', icalHttpUrl);
        this.ui.icalWebCal.attr('href', icalWebCalUrl);
    }
});