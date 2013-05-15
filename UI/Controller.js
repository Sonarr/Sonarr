"use strict";
define(['app', 'AddSeries/AddSeriesLayout',
    'Series/Index/SeriesIndexLayout',
    'Calendar/CalendarCollectionView', 'Shared/NotificationView',
    'Shared/NotFoundView', 'MainMenuView',
    'Series/Details/SeriesDetailsView', 'Series/EpisodeCollection',
    'Settings/SettingsLayout', 'Missing/MissingLayout',
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
                        NzbDrone.mainRegion.show(new NzbDrone.Series.Details.SeriesDetailsView({ model: seriesModel }));
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
            }
        });

        return new controller();

    });

