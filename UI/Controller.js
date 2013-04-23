"use strict";
define(['app', 'Shared/ModalRegion', 'AddSeries/AddSeriesLayout',
    'Series/Index/SeriesIndexLayout', 'Upcoming/UpcomingCollectionView',
    'Calendar/CalendarCollectionView', 'Shared/NotificationView',
    'Shared/NotFoundView', 'MainMenuView',
    'Series/Details/SeriesDetailsView', 'Series/EpisodeCollection',
    'Settings/SettingsLayout', 'Missing/MissingCollectionView'],
    function (app, modalRegion) {

        var controller = Backbone.Marionette.Controller.extend({

            series: function () {
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

            upcoming: function () {
                this._setTitle('Upcoming');
                NzbDrone.mainRegion.show(new NzbDrone.Upcoming.UpcomingCollectionView());
            },

            calendar: function () {
                this._setTitle('Calendar');
                var calendarCollection = new NzbDrone.Calendar.CalendarCollection();
                calendarCollection.fetch();
                NzbDrone.mainRegion.show(new NzbDrone.Calendar.CalendarCollectionView({collection: calendarCollection}));
            },


            settings: function (action) {
                this._setTitle('Settings');

                var settingsModel = new NzbDrone.Settings.SettingsModel();
                settingsModel.fetch({
                    success: function (settings) {
                        NzbDrone.mainRegion.show(new NzbDrone.Settings.SettingsLayout({settings: settings, action: action}));
                    }
                });
            },

            missing: function () {
                this._setTitle('Missing');

                var missingCollection = new NzbDrone.Missing.MissingCollection();
                missingCollection.fetch({
                    success: function () {
                        NzbDrone.mainRegion.show(new NzbDrone.Missing.MissingCollectionView({collection: missingCollection}));
                    }
                });
            },

            notFound: function () {
                this._setTitle('Not Found');
                NzbDrone.mainRegion.show(new NzbDrone.Shared.NotFoundView(this));
            },

            _setTitle: function (title) {
                $('#title-region').html(title);

                if (title.toLocaleLowerCase() === 'nzbdrone') {
                    window.document.title = 'NzbDrone';
                }
                else {
                    window.document.title = title + ' - NzbDrone';
                }
            }
        });

        //Modal dialog initializer
        NzbDrone.addInitializer(function () {

            NzbDrone.addRegions({ modalRegion: modalRegion });

            NzbDrone.vent.on(NzbDrone.Events.OpenModalDialog, function (options) {
                console.log('opening modal dialog ' + options.view.template);
                NzbDrone.modalRegion.show(options.view);
            });

            NzbDrone.vent.on(NzbDrone.Events.CloseModalDialog, function () {
                console.log('closing modal dialog');
                NzbDrone.modalRegion.close();
            });

        });

        return new controller();

    });

