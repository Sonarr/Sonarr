'use strict';

define(['app', 'Calendar/CalendarItemView'], function (app) {
    NzbDrone.Calendar.CalendarCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Calendar.CalendarItemView,
        template: 'Calendar/CalendarCollectionTemplate',
        itemViewContainer: 'table',

        ui: {
            calendar: '#calendar'
        },

        initialize: function () {
            this.collection = new NzbDrone.Calendar.CalendarCollection();
            this.collection.fetch();
            this.collection.bind('reset', this.addAll);
        },
        render: function() {
            this.ui.calendar.fullCalendar({
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'month,basicWeek,basicDay',
                    ignoreTimezone: false
                },
                selectable: true,
                selectHelper: true,
                editable: true
            });
    	},
        addAll: function(){
            this.el.fullCalendar('addEventSource', this.collection.toJSON());
        }
    });
});