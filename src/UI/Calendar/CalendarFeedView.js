'use strict';
define(
    [
        'marionette',
        'System/StatusModel',
        'Mixins/CopyToClipboard'
    ], function (Marionette, StatusModel) {
        return Marionette.Layout.extend({
            template: 'Calendar/CalendarFeedViewTemplate',

            ui: {
                icalUrl       : '.x-ical-url',
                icalCopy      : '.x-ical-copy'
            },

            templateHelpers: {
                icalHttpUrl   : window.location.protocol + '//' + window.location.host + StatusModel.get('urlBase') + '/feed/calendar/NzbDrone.ics',
                icalWebCalUrl : 'webcal://' + window.location.host + StatusModel.get('urlBase') + '/feed/calendar/NzbDrone.ics'
            },

            onShow: function () {
                this.ui.icalCopy.copyToClipboard(this.ui.icalUrl);
            }
        });
    });
