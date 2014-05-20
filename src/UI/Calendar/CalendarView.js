'use strict';

define(
    [
        'jquery',
        'vent',
        'marionette',
        'moment',
        'Calendar/Collection',
        'System/StatusModel',
        'History/Queue/QueueCollection',
        'Config',
        'Mixins/backbone.signalr.mixin',
        'fullcalendar',
        'jquery.easypiechart'
    ], function ($, vent, Marionette, moment, CalendarCollection, StatusModel, QueueCollection, Config) {

        return Marionette.ItemView.extend({
            storageKey: 'calendar.view',

            initialize: function () {
                this.collection = new CalendarCollection().bindSignalR({ updateOnly: true });
                this.listenTo(this.collection, 'change', this._reloadCalendarEvents);
                this.listenTo(QueueCollection, 'sync', this._reloadCalendarEvents);
            },

            render    : function () {
                this.$el.empty().fullCalendar(this._getOptions());
            },

            onShow: function () {
                this.$('.fc-button-today').click();
                this.$el.fullCalendar('render');

                this.$('.fc-day-header').css('width: 14.3%');
            },

            _viewRender: function (view) {
                if (Config.getValue(this.storageKey) !== view.name) {
                    Config.setValue(this.storageKey, view.name);
                }
                
                this._getEvents(view);
                this.$('.fc-day-header').css('width: 14.3%');
            },
            
            _eventRender: function (event, element) {
                this.$(element).addClass(event.statusLevel);
                this.$(element).children('.fc-event-inner').addClass(event.statusLevel);

                if (event.progress > 0) {
                    this.$(element).find('.fc-event-time')
                        .after('<span class="chart pull-right" data-percent="{0}"></span>'.format(event.progress));

                    this.$(element).find('.chart').easyPieChart({
                        barColor  : '#ffffff',
                        trackColor: false,
                        scaleColor: false,
                        lineWidth : 2,
                        size      : 14,
                        animate   : false
                    });

                    this.$(element).find('.chart').tooltip({
                        title: 'Episode is downloading - {0}% {1}'.format(event.progress.toFixed(1), event.releaseTitle)
                    });
                }
            },
            
            _getEvents: function (view) {
                var start = moment(view.visStart).toISOString();
                var end = moment(view.visEnd).toISOString();

                this.$el.fullCalendar('removeEvents');

                this.collection.fetch({
                    data   : { start: start, end: end },
                    success: this._setEventData.bind(this)
                });
            },

            _setEventData: function (collection) {
                var events = [];

                var self = this;

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
                        statusLevel : self._getStatusLevel(model, end),
                        progress    : self._getDownloadProgress(model),
                        releaseTitle: self._getReleaseTitle(model),
                        model       : model
                    };

                    events.push(event);
                });

                this.$el.fullCalendar('addEventSource', events);
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
            },

            _getReleaseTitle: function (element) {
                var downloading = QueueCollection.findEpisode(element.get('id'));

                if (!downloading) {
                    return '';
                }

                return downloading.get('title');
            },

            _getOptions: function () {
                var options = {
                    allDayDefault : false,
                    ignoreTimezone: false,
                    weekMode      : 'variable',
                    firstDay      : StatusModel.get('startOfWeek'),
                    timeFormat    : 'h(:mm)tt',
                    buttonText    : {
                        prev: '<i class="icon-arrow-left"></i>',
                        next: '<i class="icon-arrow-right"></i>'
                    },
                    viewRender    : this._viewRender.bind(this),
                    eventRender   : this._eventRender.bind(this),
                    eventClick    : function (event) {
                        vent.trigger(vent.Commands.ShowEpisodeDetails, {episode: event.model});
                    }
                };

                if ($(window).width() < 768) {
                    options.defaultView = Config.getValue(this.storageKey, 'basicDay');

                    options.titleFormat = {
                        month: 'MMM yyyy',                             // September 2009
                        week: 'MMM d[ yyyy]{ \'&#8212;\'[ MMM] d yyyy}', // Sep 7 - 13 2009
                        day: 'ddd, MMM d, yyyy'                  // Tuesday, Sep 8, 2009
                    };

                    options.header = {
                        left  : 'prev,next today',
                        center: 'title',
                        right : 'basicWeek,basicDay'
                    };
                }

                else {
                    options.defaultView = Config.getValue(this.storageKey, 'basicWeek');

                    options.titleFormat = {
                        month: 'MMM yyyy',                             // September 2009
                        week: 'MMM d[ yyyy]{ \'&#8212;\'[ MMM] d yyyy}', // Sep 7 - 13 2009
                        day: 'dddd, MMM d, yyyy'                  // Tues, Sep 8, 2009
                    };

                    options.header = {
                        left  : 'prev,next today',
                        center: 'title',
                        right : 'month,basicWeek,basicDay'
                    };
                }

                return options;
            }
        });
    });
