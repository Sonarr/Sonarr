'use strict';

define(['app', 'Calendar/CalendarItemView'], function (app) {
    NzbDrone.Calendar.CalendarCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Calendar.CalendarItemView,
        itemViewContainer: '#fakeContainer',
        template: 'Calendar/CalendarCollectionTemplate',

        ui: {
            calendar: '#calendar'
        },

        initialize: function (context, collection) {
            this.collection = collection;
        },
        onRender: function() {
            $(this.ui.calendar).fullCalendar({
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'month,basicWeek',
                    ignoreTimezone: false
                },
                buttonText: {
                    prev: '<i class="icon-arrow-left"></i>',
                    next: '<i class="icon-arrow-right"></i>'
                }
            });

            $(this.ui.calendar).fullCalendar('addEventSource', this.collection.toJSON());
    	},
        addAll: function(){
            $(this.ui.calendar).fullCalendar('addEventSource', this.collection.toJSON());
        }
    });
});