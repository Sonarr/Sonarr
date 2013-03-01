define(['app', 'Shared/ModalRegion', 'AddSeries/AddSeriesLayout', 'Series/SeriesCollectionView',
        'Upcoming/UpcomingCollectionView', 'Calendar/CalendarCollectionView', 'Shared/NotificationView',
        'Shared/NotFoundView', 'MainMenuView', 'HeaderView'], function (app, modalRegion) {

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

