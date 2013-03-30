'use strict';

define(['app', 'Calendar/CalendarItemView'], function () {
    NzbDrone.Calendar.CalendarCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView         : NzbDrone.Calendar.CalendarItemView,
        itemViewContainer: '#events',
        template         : 'Calendar/CalendarCollectionTemplate',
        className        : 'row',

        ui: {
            calendar: '#calendar'
        },

        initialize                   : function (context, action, query, collection) {
            this.collection = collection;
            this.calendar = new NzbDrone.Calendar.CalendarCollection();
        },
        onCompositeCollectionRendered: function () {
            $(this.ui.calendar).fullCalendar({
                allDayDefault : false,
                ignoreTimezone: false,
                weekMode      : 'variable',
                timeFormat    : 'h(:mm)tt',
                header        : {
                    left  : 'prev,next today',
                    center: 'title',
                    right : 'month,basicWeek'
                },
                buttonText    : {
                    prev: '<i class="icon-arrow-left"></i>',
                    next: '<i class="icon-arrow-right"></i>'
                },
                events        : this.getEvents,
                eventRender   : function (event, element) {
                    $(element).addClass(event.statusLevel);
                    $(element).children('.fc-event-inner').addClass(event.statusLevel);

                    element.popover({
                        title    : '{seriesTitle} - {season}x{episode} - {episodeTitle}'.assign({
                            seriesTitle : event.seriesTitle,
                            season      : event.seasonNumber,
                            episode     : event.episodeNumber.pad(2),
                            episodeTitle: event.episodeTitle
                        }),
                        content  : event.overview,
                        placement: 'bottom',
                        trigger  : 'manual'
                    });
                },
                eventMouseover: function () {
                    $(this).popover('show');
                },
                eventMouseout : function () {
                    $(this).popover('hide');
                }
            });

            NzbDrone.Calendar.CalendarCollectionView.Instance = this;
            $(this.ui.calendar).fullCalendar('addEventSource', this.calendar.toJSON());
        },
        getEvents                    : function (start, end, callback) {
            var bbView = NzbDrone.Calendar.CalendarCollectionView.Instance;

            var startDate = Date.create(start).format(Date.ISO8601_DATETIME);
            var endDate = Date.create(end).format(Date.ISO8601_DATETIME);

            bbView.calendar.fetch({
                data   : { start: startDate, end: endDate },
                success: function (calendarCollection) {
                    callback(calendarCollection.toJSON());
                }
            });
        }
    });
});