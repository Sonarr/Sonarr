'use strict';

define(['app', 'Calendar/CalendarItemView'], function (app) {
    NzbDrone.Calendar.CalendarCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Calendar.CalendarItemView,
        itemViewContainer: '#upcomingContainer',
        template: 'Calendar/CalendarCollectionTemplate',

        ui: {
            calendar: '#calendar'
        },

        initialize: function (context, collection) {
            this.collection = collection;
            this.calendar = new NzbDrone.Calendar.CalendarCollection();
        },
        onCompositeCollectionRendered: function() {
            $(this.ui.calendar).fullCalendar({
                allDayDefault: false,
                //ignoreTimezone: false,
                weekMode: 'variable',
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'month,basicWeek'
                },
                buttonText: {
                    prev: '<i class="icon-arrow-left"></i>',
                    next: '<i class="icon-arrow-right"></i>'
                },
                events: this.getEvents
            });

            NzbDrone.Calendar.CalendarCollectionView.Instance = this;
            $(this.ui.calendar).fullCalendar('addEventSource', this.calendar.toJSON());
    	},
        getEvents: function(start, end, callback){
            var bbView = NzbDrone.Calendar.CalendarCollectionView.Instance;



            bbView.calendar.fetch({
                data:{ start: Date.create(start).format(Date.ISO8601_DATETIME), end: Date.create(end).format(Date.ISO8601_DATETIME) },
                success:function (calendarCollection) {
                    callback(calendarCollection.toJSON());
                }
            });
        }
    });
});