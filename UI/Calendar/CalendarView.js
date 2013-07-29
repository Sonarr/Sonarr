'use strict';

define(
    [
        'app',
        'marionette',
        'moment',
        'Calendar/Collection',
        'fullcalendar'
    ], function (App, Marionette, Moment, CalendarCollection) {

        var _instance;

        return Marionette.ItemView.extend({
            initialize: function () {
                this.collection = new CalendarCollection();
            },
            render    : function () {
                $(this.$el).empty().fullCalendar({
                    defaultView   : 'basicWeek',
                    allDayDefault : false,
                    ignoreTimezone: false,
                    weekMode      : 'variable',
                    timeFormat    : 'h(:mm)tt',
                    header        : {
                        left  : 'prev,next today',
                        center: 'title',
                        right : 'month,basicWeek,basicDay'
                    },
                    buttonText    : {
                        prev: '<i class="icon-arrow-left"></i>',
                        next: '<i class="icon-arrow-right"></i>'
                    },
                    events        : this.getEvents,
                    eventRender   : function (event, element) {
                        $(element).addClass(event.statusLevel);
                        $(element).children('.fc-event-inner').addClass(event.statusLevel);
                    },
                    eventClick    : function (event) {
                        App.vent.trigger(App.Commands.ShowEpisodeDetails, {episode: event.model});
                    }
                });

                _instance = this;
            },

            onShow: function () {
                this.$('.fc-button-today').click();
            },

            getEvents: function (start, end, callback) {
                var startDate = Moment(start).toISOString();
                var endDate = Moment(end).toISOString();

                _instance.collection.fetch({
                    data   : { start: startDate, end: endDate },
                    success: function (calendarCollection) {
                        _.each(calendarCollection.models, function (element) {
                            var episodeTitle = element.get('title');
                            var seriesTitle = element.get('series').get('title');
                            var start = element.get('airDateUtc');
                            var statusLevel = _instance.getStatusLevel(element);

                            element.set({
                                title       : seriesTitle,
                                episodeTitle: episodeTitle,
                                start       : start,
                                allDay      : false,
                                statusLevel : statusLevel
                            });

                            element.set('model', element);
                        });

                        callback(calendarCollection.toJSON());
                    }
                });
            },

            getStatusLevel: function (element) {
                var hasFile = element.get('hasFile');
                var currentTime = Moment();
                var start = Moment(element.get('airDateUtc'));
                var end = Moment(element.get('end'));

                var statusLevel = 'primary';

                if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
                    statusLevel = 'warning';
                }

                else if (start.isBefore(currentTime) && !hasFile) {
                    statusLevel = 'danger';
                }

                else if (hasFile) {
                    statusLevel = 'success';
                }

                var test = currentTime.startOf('day').format('LLLL');

                if (end.isBefore(currentTime.startOf('day'))) {
                    statusLevel += ' past'
                }

                return statusLevel;
            }
        });
    });
