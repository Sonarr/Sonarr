"use strict";
define(['app',
    'Form/FormBuilder',
    'AddSeries/AddSeriesLayout',
    'Series/Index/SeriesIndexLayout',
    'Calendar/CalendarCollectionView',
    'Shared/NotificationView',
    'Shared/NotFoundView',
    'MainMenuView',
    'Series/Details/SeriesDetailsLayout',
    'Series/EpisodeCollection',
    'Settings/SettingsLayout',
    'Logs/Layout',
    'Release/Layout',
    'Missing/MissingLayout',
    'History/HistoryLayout'],
    function () {
        var controller = Backbone.Marionette.Controller.extend({

            series       : function () {
                this._setTitle('NzbDrone');
                NzbDrone.mainRegion.show(new NzbDrone.Series.Index.SeriesIndexLayout());
            },
            seriesDetails: function (query) {

                var self = this;
                this._setTitle('Loading Series');
                var series = new NzbDrone.Series.SeriesModel({ id: query });
                series.fetch({
                    success: function (seriesModel) {
                        self._setTitle(seriesModel.get('title'));
                        NzbDrone.mainRegion.show(new NzbDrone.Series.Details.SeriesDetailsLayout({ model: seriesModel }));
                    }
                });
            },

            addSeries: function (action) {
                this._setTitle('Add Series');
                NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout({action: action}));
            },

            calendar: function () {
                this._setTitle('Calendar');
                var calendarCollection = new NzbDrone.Calendar.CalendarCollection();
                calendarCollection.fetch();
                NzbDrone.mainRegion.show(new NzbDrone.Calendar.CalendarCollectionView({collection: calendarCollection}));
            },


            settings: function (action) {
                this._setTitle('Settings');
                NzbDrone.mainRegion.show(new NzbDrone.Settings.SettingsLayout({action: action}));
            },

            missing: function () {
                this._setTitle('Missing');

                NzbDrone.mainRegion.show(new NzbDrone.Missing.MissingLayout());
            },

            history: function () {
                this._setTitle('History');

                NzbDrone.mainRegion.show(new NzbDrone.History.HistoryLayout());
            },

            rss: function () {
                this._setTitle('RSS');
                NzbDrone.mainRegion.show(new NzbDrone.Release.Layout());
            },

            logs: function () {
                this._setTitle('logs');
                NzbDrone.mainRegion.show(new NzbDrone.Logs.Layout());
            },

            notFound: function () {
                this._setTitle('Not Found');
                NzbDrone.mainRegion.show(new NzbDrone.Shared.NotFoundView(this));
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

                var cookies = document.cookie.split(";");

                for (var i = 0; i < cookies.length; i++) {
                    var cookie = cookies[i];
                    var eqPos = cookie.indexOf("=");
                    var name = eqPos > -1 ? cookie.substr(0, eqPos) :cookie;
                    document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
                }
            }
        });

        return new controller();

    });

