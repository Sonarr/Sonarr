define(['app', 'Shared/ModalRegion', 'AddSeries/AddSeriesLayout',
        'Series/SeriesCollectionView', 'Upcoming/UpcomingCollectionView',
        'Calendar/CalendarCollectionView', 'Shared/NotificationView',
        'Shared/NotFoundView', 'MainMenuView', 'HeaderView',
        'Series/Details/SeriesDetailsView', 'Series/Details/EpisodeCollection'],
        function (app, modalRegion) {

    var controller = Backbone.Marionette.Controller.extend({

        addSeries: function (action, query) {
            this.setTitle('Add Series');
            NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout(this, action, query));
        },

        series: function (action, query) {
            this.setTitle('NzbDrone');
            NzbDrone.mainRegion.show(new NzbDrone.Series.SeriesCollectionView());
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
            this.setTitle('Series Title Goes Here');
//            var seriesModel = new NzbDrone.Series.SeriesModel();
//            seriesModel.fetch();

            var seriesEpisodes = new NzbDrone.Series.Details.EpisodeCollection({ seriesId: query });
            seriesEpisodes.fetch({
                success: function (collection) {
                    var seasons = collection.models.groupBy(function(episode){
                        var seasonNumber = episode.get('seasonNumber');

                        if (seasonNumber === undefined)
                            return 0;

                        return seasonNumber;
                    });

                    var seasonCollection = new NzbDrone.Series.Details.SeasonCollection();

                    $.each(seasons, function(index, season){
                       seasonCollection.add(new NzbDrone.Series.Details.SeasonModel(
                           { seasonNumber: index, episodes: season })
                       );
                    });

                    NzbDrone.mainRegion.show(new NzbDrone.Series.Details.SeriesDetailsView({ collection: seasonCollection }));
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

