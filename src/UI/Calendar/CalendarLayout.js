'use strict';
define(
    [
        'marionette',
        'Calendar/UpcomingCollectionView',
        'Calendar/CalendarView'
    ], function (Marionette, UpcomingCollectionView, CalendarView) {
        return Marionette.Layout.extend({
            template: 'Calendar/CalendarLayoutTemplate',

            regions: {
                upcoming: '#x-upcoming',
                calendar: '#x-calendar'
            },

            onShow: function () {
                this._showUpcoming();
                this._showCalendar();
            },

            _showUpcoming: function () {
                this.upcoming.show(new UpcomingCollectionView());
            },

            _showCalendar: function () {
                this.calendar.show(new CalendarView());
            }
        });
    });
