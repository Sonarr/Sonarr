'use strict';

define(
    [
        'vent',
        'marionette',
        'moment',
        'Calendar/Collection',
        'System/StatusModel',
        'History/Queue/QueueCollection',
        'Mixins/backbone.signalr.mixin',
        'fullcalendar',
        'jquery.easypiechart'
    ], function (vent, Marionette, moment, CalendarCollection, StatusModel, QueueCollection) {

        var _instance;

        return Marionette.ItemView.extend({
            initialize: function () {
                this.collection = new CalendarCollection().bindSignalR({ updateOnly: true });
                this.listenTo(this.collection, 'change', this._reloadCalendarEvents);
                this.listenTo(QueueCollection, 'sync', this._reloadCalendarEvents);
            },
            render    : function () {

                var self = this;

                this.$el.empty().fullCalendar({
                    defaultView   : 'basicWeek',
                    allDayDefault : false,
                    ignoreTimezone: false,
                    weekMode      : 'variable',
                    firstDay      : StatusModel.get('startOfWeek'),
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
                    viewRender : this._getEvents,
                    eventRender   : function (event, element) {
                        self.$(element).addClass(event.statusLevel);
                        self.$(element).children('.fc-event-inner').addClass(event.statusLevel);

                        if (event.progress > 0) {
                            self.$(element).find('.fc-event-time')
                                .after('<span class="chart pull-right" data-percent="{0}"></span>'.format(event.progress));

                            self.$(element).find('.chart').easyPieChart({
                                barColor  : '#ffffff',
                                trackColor: false,
                                scaleColor: false,
                                lineWidth : 2,
                                size      : 14,
                                animate   : false
                            });
                        }
                    },
                    eventClick    : function (event) {
                        vent.trigger(vent.Commands.ShowEpisodeDetails, {episode: event.model});
                    }
                });

                _instance = this;
            },

            onShow: function () {
                this.$('.fc-button-today').click();
            },

            _getEvents: function (view) {
                var start = moment(view.visStart).toISOString();
                var end = moment(view.visEnd).toISOString();

                _instance.$el.fullCalendar('removeEvents');

                _instance.collection.fetch({
                    data   : { start: start, end: end },
                    success: function (collection) {
                        _instance._setEventData(collection);
                    }
                });
            },

            _setEventData: function (collection) {
                var events = [];

                collection.each(function (model) {
                    var seriesTitle = model.get('series').title;
                    var start = model.get('airDateUtc');
                    var runtime = model.get('series').runtime;
                    var end = moment(start).add('minutes', runtime).toISOString();

                    var event = {
                        title       : seriesTitle,
                        start       : start,
                        end         : end,
                        allDay      : false,
                        statusLevel : _instance._getStatusLevel(model, end),
                        progress    : _instance._getDownloadProgress(model),
                        model       : model
                    };

                    events.push(event);
                });

                _instance.$el.fullCalendar('addEventSource', events);
            },

            _getStatusLevel: function (element, endTime) {
                var hasFile = element.get('hasFile');
                var downloading = QueueCollection.findEpisode(element.get('id')) || element.get('downloading');
                var currentTime = moment();
                var start = moment(element.get('airDateUtc'));
                var end = moment(endTime);

                var statusLevel = 'primary';

                if (hasFile) {
                    statusLevel = 'success';
                }

                else if (downloading) {
                    statusLevel = 'purple';
                }

                else if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
                    statusLevel = 'warning';
                }

                else if (start.isBefore(currentTime) && !hasFile) {
                    statusLevel = 'danger';
                }

                if (end.isBefore(currentTime.startOf('day'))) {
                    statusLevel += ' past';
                }

                return statusLevel;
            },

            _reloadCalendarEvents: function () {
                this.$el.fullCalendar('removeEvents');
                this._setEventData(this.collection);
            },

            _getDownloadProgress: function (element) {
                var downloading = QueueCollection.findEpisode(element.get('id'));

                if (!downloading) {
                    return 0;
                }

                return 100 - (downloading.get('sizeleft') / downloading.get('size') * 100);
            }
        });
    });
