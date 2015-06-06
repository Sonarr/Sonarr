var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var moment = require('moment');
var CalendarCollection = require('./CalendarCollection');
var UiSettings = require('../Shared/UiSettingsModel');
var QueueCollection = require('../Activity/Queue/QueueCollection');
var Config = require('../Config');

require('../Mixins/backbone.signalr.mixin');
require('fullcalendar');
require('jquery.easypiechart');

module.exports = Marionette.ItemView.extend({
    storageKey : 'calendar.view',

    initialize : function() {
        this.showUnmonitored = Config.getValue('calendar.show', 'monitored') === 'all';
        this.collection = new CalendarCollection().bindSignalR({ updateOnly : true });
        this.listenTo(this.collection, 'change', this._reloadCalendarEvents);
        this.listenTo(QueueCollection, 'sync', this._reloadCalendarEvents);
    },

    render : function() {
        this.$el.empty().fullCalendar(this._getOptions());
    },

    onShow : function() {
        this.$('.fc-button-today').click();
    },

    setShowUnmonitored : function (showUnmonitored) {
        if (this.showUnmonitored !== showUnmonitored) {
            this.showUnmonitored = showUnmonitored;
            this._getEvents(this.$el.fullCalendar('getView'));
        }
    },

    _viewRender : function(view) {
        if ($(window).width() < 768) {
            this.$('.fc-header-title').show();
            this.$('.calendar-title').remove();

            var title = this.$('.fc-header-title').text();
            var titleDiv = '<div class="calendar-title"><h2>{0}</h2></div>'.format(title);

            this.$('.fc-header').before(titleDiv);
            this.$('.fc-header-title').hide();
        }

        if (Config.getValue(this.storageKey) !== view.name) {
            Config.setValue(this.storageKey, view.name);
        }

        this._getEvents(view);
    },

    _eventRender : function(event, element) {
        this.$(element).addClass(event.statusLevel);
        this.$(element).children('.fc-event-inner').addClass(event.statusLevel);

        if (event.downloading) {
            var progress = 100 - event.downloading.get('sizeleft') / event.downloading.get('size') * 100;
            var releaseTitle = event.downloading.get('title');
            var estimatedCompletionTime = moment(event.downloading.get('estimatedCompletionTime')).fromNow();
            var status = event.downloading.get('status').toLocaleLowerCase();
            var errorMessage = event.downloading.get('errorMessage');

            if (status === 'pending') {
                this._addStatusIcon(element, 'icon-sonarr-pending', 'Release will be processed {0}'.format(estimatedCompletionTime));
            }

            else if (errorMessage) {
                if (status === 'completed') {
                    this._addStatusIcon(element, 'icon-sonarr-import-failed', 'Import failed: {0}'.format(errorMessage));
                } else {
                    this._addStatusIcon(element, 'icon-sonarr-download-failed', 'Download failed: {0}'.format(errorMessage));
                }
            }

            else if (status === 'failed') {
                this._addStatusIcon(element, 'icon-sonarr-download-failed', 'Download failed: check download client for more details');
            }

            else if (status === 'warning') {
                this._addStatusIcon(element, 'icon-sonarr-download-warning', 'Download warning: check download client for more details');
            }

            else {
                this.$(element).find('.fc-event-time').after('<span class="chart pull-right" data-percent="{0}"></span>'.format(progress));

                this.$(element).find('.chart').easyPieChart({
                    barColor   : '#ffffff',
                    trackColor : false,
                    scaleColor : false,
                    lineWidth  : 2,
                    size       : 14,
                    animate    : false
                });

                this.$(element).find('.chart').tooltip({
                    title     : 'Episode is downloading - {0}% {1}'.format(progress.toFixed(1), releaseTitle),
                    container : '.fc-content'
                });
            }
        }
    },

    _getEvents : function(view) {
        var start = moment(view.start.toISOString()).toISOString();
        var end = moment(view.end.toISOString()).toISOString();

        this.$el.fullCalendar('removeEvents');

        this.collection.fetch({
            data    : {
                start       : start,
                end         : end,
                unmonitored : this.showUnmonitored
            },
            success : this._setEventData.bind(this)
        });
    },

    _setEventData : function(collection) {
        var events = [];

        var self = this;

        collection.each(function(model) {
            var seriesTitle = model.get('series').title;
            var start = model.get('airDateUtc');
            var runtime = model.get('series').runtime;
            var end = moment(start).add('minutes', runtime).toISOString();

            var event = {
                title       : seriesTitle,
                start       : moment(start),
                end         : moment(end),
                allDay      : false,
                statusLevel : self._getStatusLevel(model, end),
                downloading : QueueCollection.findEpisode(model.get('id')),
                model       : model
            };

            events.push(event);
        });

        this.$el.fullCalendar('addEventSource', events);
    },

    _getStatusLevel : function(element, endTime) {
        var hasFile = element.get('hasFile');
        var downloading = QueueCollection.findEpisode(element.get('id')) || element.get('grabbed');
        var currentTime = moment();
        var start = moment(element.get('airDateUtc'));
        var end = moment(endTime);
        var monitored = element.get('series').monitored && element.get('monitored');

        var statusLevel = 'primary';

        if (hasFile) {
            statusLevel = 'success';
        }

        else if (downloading) {
            statusLevel = 'purple';
        }

        else if (!monitored) {
            statusLevel = 'unmonitored';
        }

        else if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
            statusLevel = 'warning';
        }

        else if (start.isBefore(currentTime) && !hasFile) {
            statusLevel = 'danger';
        }

        else if (element.get('episodeNumber') === 1) {
            statusLevel = 'premiere';
        }

        if (end.isBefore(currentTime.startOf('day'))) {
            statusLevel += ' past';
        }

        return statusLevel;
    },

    _reloadCalendarEvents : function() {
        this.$el.fullCalendar('removeEvents');
        this._setEventData(this.collection);
    },

    _getOptions    : function() {
        var options = {
            allDayDefault : false,
            weekMode      : 'variable',
            firstDay      : UiSettings.get('firstDayOfWeek'),
            timeFormat    : 'h(:mm)a',
            viewRender    : this._viewRender.bind(this),
            eventRender   : this._eventRender.bind(this),
            eventClick    : function(event) {
                vent.trigger(vent.Commands.ShowEpisodeDetails, { episode : event.model });
            }
        };

        if ($(window).width() < 768) {
            options.defaultView = Config.getValue(this.storageKey, 'basicDay');

            options.header = {
                left   : 'prev,next today',
                center : 'title',
                right  : 'basicWeek,basicDay'
            };
        }

        else {
            options.defaultView = Config.getValue(this.storageKey, 'basicWeek');

            options.header = {
                left   : 'prev,next today',
                center : 'title',
                right  : 'month,basicWeek,basicDay'
            };
        }

        options.titleFormat = {
            month : 'MMMM YYYY',
            week  : UiSettings.get('shortDateFormat'),
            day   : UiSettings.get('longDateFormat')
        };

        options.columnFormat = {
            month : 'ddd',
            week  : UiSettings.get('calendarWeekColumnHeader'),
            day   : 'dddd'
        };

        options.timeFormat = {
            'default' : UiSettings.get('timeFormat')
        };

        return options;
    },

    _addStatusIcon : function(element, icon, tooltip) {
        this.$(element).find('.fc-event-time').after('<span class="status pull-right"><i class="{0}"></i></span>'.format(icon));
        this.$(element).find('.status').tooltip({
            title     : tooltip,
            container : '.fc-content'
        });
    }
});