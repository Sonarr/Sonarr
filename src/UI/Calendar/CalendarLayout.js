'use strict';
define(
    [
        'AppLayout',
        'marionette',
        'Calendar/UpcomingCollectionView',
        'Calendar/CalendarView',
        'Calendar/CalendarFeedView'
    ], function (AppLayout, Marionette, UpcomingCollectionView, CalendarView, CalendarFeedView) {
        return Marionette.Layout.extend({
            template: 'Calendar/CalendarLayoutTemplate',

            regions: {
                upcoming: '#x-upcoming',
                calendar: '#x-calendar'
            },
            
            events: {
                'click .x-ical': '_showiCal'
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
            },
            
            _showiCal: function () {
                var view = new CalendarFeedView();
                AppLayout.modalRegion.show(view);
            }
        });
    });
