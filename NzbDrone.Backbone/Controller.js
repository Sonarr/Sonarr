define(['app', 'Shared/ModalRegion', 'AddSeries/AddSeriesLayout',
        'Series/Index/SeriesIndexCollectionView', 'Upcoming/UpcomingCollectionView',
        'Calendar/CalendarCollectionView', 'Shared/NotificationView',
        'Shared/NotFoundView', 'MainMenuView', 'HeaderView',
        'Series/Details/SeriesDetailsView', 'Series/EpisodeCollection'],
        function (app, modalRegion) {

    var controller = Backbone.Marionette.Controller.extend({

        addSeries: function (action, query) {
            this.setTitle('Add Series');
            NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout(this, action, query));
        },

        series: function (action, query) {
            this.setTitle('NzbDrone');
            NzbDrone.mainRegion.show(new NzbDrone.Series.Index.SeriesIndexCollectionView());
        },

        upcoming: function (action, query) {
            this.setTitle('Upcoming');
            NzbDrone.mainRegion.show(new NzbDrone.Upcoming.UpcomingCollectionView(this, action, query));
        },
        
        calendar: function (action, query) {
            this.setTitle('Calendar');
            var calendarCollection = new NzbDrone.Calendar.CalendarCollection();
            calendarCollection.fetch();
            NzbDrone.mainRegion.show(new NzbDrone.Calendar.CalendarCollectionView(this, action, query, calendarCollection));
        },

        seriesDetails: function (query) {

            var self = this;
            this.setTitle('Loading Series');
            var series = new NzbDrone.Series.SeriesModel({ id: query });
            series.fetch({
                success: function (seriesModel) {
                    self.setTitle(seriesModel.get('title'));
                    NzbDrone.mainRegion.show(new NzbDrone.Series.Details.SeriesDetailsView({ model: seriesModel }));
                }
            });
        },

        notFound: function () {
            this.setTitle('Not Found');
            NzbDrone.mainRegion.show(new NzbDrone.Shared.NotFoundView(this));
        },

        setTitle: function (title) {
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

