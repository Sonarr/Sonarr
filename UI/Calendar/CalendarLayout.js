"use strict";
define([
    'app',
    'Calendar/UpcomingCollectionView',
    'Calendar/CalendarView',
    'Shared/Toolbar/ToolbarLayout'
],
    function () {
        NzbDrone.Calendar.CalendarLayout = Backbone.Marionette.Layout.extend({
            template: 'Calendar/CalendarLayoutTemplate',

            regions: {
                upcoming: '#x-upcoming',
                calendar: '#x-calendar'
            },

            initialize: function () {
                this.upcomingCollection = new NzbDrone.Calendar.UpcomingCollection();
                this.upcomingCollection.fetch();
            },

            onShow: function () {
                this._showUpcoming();
                this._showCalendar();
            },

            _showUpcoming: function () {
                this.upcoming.show(new NzbDrone.Calendar.UpcomingCollectionView({
                    collection: this.upcomingCollection
                }));
            },

            _showCalendar: function () {
                this.calendar.show(new NzbDrone.Calendar.CalendarView());
            }
        });
    });
