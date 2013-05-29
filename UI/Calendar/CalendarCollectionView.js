'use strict';

define(['app', 'Calendar/CalendarItemView'], function () {
    NzbDrone.Calendar.CalendarCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView         : NzbDrone.Calendar.CalendarItemView,
        itemViewContainer: '#events',
        template         : 'Calendar/CalendarCollectionTemplate',

        ui: {
            calendar: '#calendar'
        },

        initialize : function () {
            //should use this.collection?
            this.calendar = new NzbDrone.Calendar.CalendarCollection();
        },
        onCompositeCollectionRendered: function () {
            $(this.ui.calendar).empty().fullCalendar({
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
                            seriesTitle : event.title,
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
        },

        getEvents                    : function (start, end, callback) {
            var bbView = NzbDrone.Calendar.CalendarCollectionView.Instance;

            var startDate = Date.create(start).format(Date.ISO8601_DATETIME);
            var endDate = Date.create(end).format(Date.ISO8601_DATETIME);

            bbView.calendar.fetch({
                data   : { start: startDate, end: endDate },
                success: function (calendarCollection) {
                    _.each(calendarCollection.models, function(element) {
                        var episodeTitle = element.get('title');
                        var seriesTitle = element.get('series').title;
                        var start = element.get('airDate');
                        var end = element.get('endTime');

                        element.set('title', seriesTitle);
                        element.set('episodeTitle', episodeTitle);
                        element.set('start', start);
                        element.set('end', end);
                        element.set('allDay', false);
                    });

                    callback(calendarCollection.toJSON());
                }
            });
        }
    });
});