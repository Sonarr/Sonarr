'use strict';
define(
    [
        'app',
        'marionette',
        'History/HistoryLayout',
        'Settings/SettingsLayout',
        'AddSeries/AddSeriesLayout',
        'Series/Index/SeriesIndexLayout',
        'Series/Details/SeriesDetailsLayout',
        'Series/SeriesCollection',
        'Missing/MissingLayout',
        'Series/SeriesModel',
        'Calendar/CalendarLayout',
        'Logs/Layout',
        'Logs/Files/Layout',
        'Release/Layout',
        'System/Layout',
        'SeasonPass/Layout',
        'Shared/NotFoundView',
        'Shared/Modal/Region'
    ], function (App, Marionette, HistoryLayout, SettingsLayout, AddSeriesLayout, SeriesIndexLayout, SeriesDetailsLayout, SeriesCollection, MissingLayout, SeriesModel, CalendarLayout,
        LogsLayout, LogFileLayout, ReleaseLayout, SystemLayout, SeasonPassLayout, NotFoundView) {
        return Marionette.Controller.extend({

            series: function () {
                this._setTitle('NzbDrone');
                App.mainRegion.show(new SeriesIndexLayout());
            },

            seriesDetails: function (query) {
                var series = SeriesCollection.where({titleSlug: query});

                if (series.length != 0) {
                    var targetSeries = series[0];
                    this._setTitle(targetSeries.get('title'));
                    App.mainRegion.show(new SeriesDetailsLayout({ model: targetSeries }));
                }
                else {
                    this.notFound();
                }
            },

            addSeries: function (action) {
                this._setTitle('Add Series');
                App.mainRegion.show(new AddSeriesLayout({action: action}));
            },

            calendar: function () {
                this._setTitle('Calendar');
                App.mainRegion.show(new CalendarLayout());
            },

            settings: function (action) {
                this._setTitle('Settings');
                App.mainRegion.show(new SettingsLayout({action: action}));
            },

            missing: function () {
                this._setTitle('Missing');

                App.mainRegion.show(new MissingLayout());
            },

            history: function () {
                this._setTitle('History');

                App.mainRegion.show(new HistoryLayout());
            },

            rss: function () {
                this._setTitle('RSS');
                App.mainRegion.show(new ReleaseLayout());
            },

            logs: function (action) {
                if (action) {
                    this._setTitle('log files');
                    App.mainRegion.show(new LogFileLayout());
                }

                else {
                    this._setTitle('logs');
                    App.mainRegion.show(new LogsLayout());
                }
            },

            system: function () {
                this._setTitle('system');
                App.mainRegion.show(new SystemLayout());
            },

            seasonPass: function () {
                this._setTitle('Season Pass');
                App.mainRegion.show(new SeasonPassLayout());
            },

            notFound: function () {
                this._setTitle('Not Found');
                App.mainRegion.show(new NotFoundView(this));
            },

            _setTitle: function (title) {
                //$('#title-region').html(title);

                if (title.toLocaleLowerCase() === 'nzbdrone') {
                    window.document.title = 'NzbDrone';
                }
                else {
                    window.document.title = title + ' - NzbDrone';
                }

                this._clearCookies();
            },

            _clearCookies: function () {

                if (!document.cookie) {
                    return;
                }

                var cookies = document.cookie.split(';');

                for (var i = 0; i < cookies.length; i++) {
                    var cookie = cookies[i];
                    var eqPos = cookie.indexOf('=');
                    var name = eqPos > -1 ? cookie.substr(0, eqPos) :cookie;
                    document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT';
                }
            }
        });
    });

