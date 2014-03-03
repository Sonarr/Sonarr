'use strict';
define(
    [
        'marionette',
    ], function (Marionette) {
        return Marionette.Layout.extend({
            template: 'Calendar/CalendarFeedViewTemplate',
			
			onRender: function() {
				// hackish way to determine the correct url, as using urlBase seems to only work for reverse proxies or so
				var ics = '//' + window.location.host + '/feed/calendar/NzbDrone.ics';
				this.$('#ical-url').val(window.location.protocol + ics);
				this.$('#ical-subscribe-button').attr('href', 'webcal:' + ics);
			}
        });
    });
