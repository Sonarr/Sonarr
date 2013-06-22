'use strict';

define(['app', 'Calendar/Collection','fullcalendar'], function () {
    NzbDrone.Calendar.CalendarView = Backbone.Marionette.ItemView.extend({
        initialize                   : function () {
            this.collection = new NzbDrone.Calendar.Collection();
        },
        render: function () {
            $(this.$el).empty().fullCalendar({
                defaultView   : 'basicWeek',
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
                    prev: '<i class='icon-arrow-left'></i>',
                    next: '<i class='icon-arrow-right'></i>'
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

            NzbDrone.Calendar.CalendarView.Instance = this;
        },

        onShow: function () {
            this.$('.fc-button-today').click();
        },

        getEvents: function (start, end, callback) {
            var bbView = NzbDrone.Calendar.CalendarView.Instance;

            var startDate = Date.create(start).format(Date.ISO8601_DATETIME);
            var endDate = Date.create(end).format(Date.ISO8601_DATETIME);

            bbView.collection.fetch({
                data   : { start: startDate, end: endDate },
                success: function (calendarCollection) {
                    _.each(calendarCollection.models, function (element) {
                        var episodeTitle = element.get('title');
                        var seriesTitle = element.get('series').get('title');
                        var start = element.get('airDate');

                        element.set('title', seriesTitle);
                        element.set('episodeTitle', episodeTitle);
                        element.set('start', start);
                        element.set('allDay', false);
                    });

                    callback(calendarCollection.toJSON());
                }
            });
        }
    });
});
