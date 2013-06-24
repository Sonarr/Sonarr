'use strict';
define(
    [
        'app',
        'Settings/SettingsLayout',
        'AddSeries/AddSeriesLayout',
        'Missing/MissingLayout',
        'History/HistoryLayout',
        'Form/FormBuilder',
        'Series/Index/SeriesIndexLayout',
        'Calendar/CalendarLayout',
        'Shared/NotificationView',
        'Shared/NotFoundView',
        'MainMenuView',
        'Series/Details/SeriesDetailsLayout',
        'Series/EpisodeCollection',
        'Logs/Layout',
        'Release/Layout',
        'Shared/FormatHelpers',
        'Shared/TemplateHelpers',
        'Shared/Footer/View'
    ], function (App, SettingsLayout, AddSeriesLayout, MissingLayout, HistoryLayout) {
        var controller = Backbone.Marionette.Controller.extend({

            series       : function () {
                this._setTitle('NzbDrone');
                App.mainRegion.show(new NzbDrone.Series.Index.SeriesIndexLayout());
            },
            seriesDetails: function (query) {

                var self = this;
                this._setTitle('Loading Series');
                var series = new NzbDrone.Series.SeriesModel({ id: query });
                series.fetch({
                    success: function (seriesModel) {
                        self._setTitle(seriesModel.get('title'));
                        App.mainRegion.show(new NzbDrone.Series.Details.SeriesDetailsLayout({ model: seriesModel }));
                    }
                });
            },

            addSeries: function (action) {
                this._setTitle('Add Series');
                App.mainRegion.show(new AddSeriesLayout({action: action}));
            },

            calendar: function () {
                this._setTitle('Calendar');
                App.mainRegion.show(new NzbDrone.Calendar.CalendarLayout());
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
                App.mainRegion.show(new NzbDrone.Release.Layout());
            },

            logs: function () {
                this._setTitle('logs');
                App.mainRegion.show(new NzbDrone.Logs.Layout());
            },

            notFound: function () {
                this._setTitle('Not Found');
                App.mainRegion.show(new NzbDrone.Shared.NotFoundView(this));
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

        return new controller();

    });

