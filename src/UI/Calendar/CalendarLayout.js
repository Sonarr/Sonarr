'use strict';
define(
    [
        'marionette',
        'Calendar/UpcomingCollection',
        'Calendar/UpcomingCollectionView',
        'Calendar/CalendarView',
    ], function (Marionette, UpcomingCollection, UpcomingCollectionView, CalendarView) {
        return Marionette.Layout.extend({
            template: 'Calendar/CalendarLayoutTemplate',

            regions: {
                upcoming: '#x-upcoming',
                calendar: '#x-calendar'
            },

            initialize: function () {
                this.upcomingCollection = new UpcomingCollection();
                this.upcomingCollection.fetch();
            },

            onShow: function () {
                this._showUpcoming();
                this._showCalendar();
            },

            _showUpcoming: function () {
                this.upcoming.show(new UpcomingCollectionView({
                    collection: this.upcomingCollection
                }));
            },

            _showCalendar: function () {
                this.calendar.show(new CalendarView());
            }
        });
    });
