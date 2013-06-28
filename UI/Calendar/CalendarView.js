﻿'use strict';

define(
    [
        'app',
        'marionette',
        'Calendar/Collection',
        'Episode/Layout',
        'fullcalendar'
    ], function (App, Marionette, CalendarCollection, EpisodeLayout) {

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
                    },
                    eventClick : function (event) {
                        var view = new EpisodeLayout({ model: event.model });
                        App.modalRegion.show(view);
                    }
                });

                _instance = this;
            },

            onShow: function () {
                this.$('.fc-button-today').click();
            },

            getEvents: function (start, end, callback) {
                var startDate = Date.create(start).format(Date.ISO8601_DATETIME);
                var endDate = Date.create(end).format(Date.ISO8601_DATETIME);

                _instance.collection.fetch({
                    data   : { start: startDate, end: endDate },
                    success: function (calendarCollection) {
                        _.each(calendarCollection.models, function (element) {
                            var episodeTitle = element.get('title');
                            var seriesTitle = element.get('series').get('title');
                            var start = element.get('airDate');

                            element.set({
                                'title': seriesTitle,
                                episodeTitle: episodeTitle,
                                start: start,
                                allDay: false
                            });

                            element.set('model', element);
                        });

                        callback(calendarCollection.toJSON());
                    }
                });
            }
        });
    });
