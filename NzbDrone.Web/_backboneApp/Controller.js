define(['app', 'Shared/ModalRegion', 'AddSeries/AddSeriesLayout','Series/SeriesCollectionView', 'Shared/NotificationView'], function (app, modalRegion) {

   var  controller = Backbone.Marionette.Controller.extend({

        addSeries: function (action, query) {
            NzbDrone.mainRegion.show(new NzbDrone.AddSeries.AddSeriesLayout(this, action, query));
            this.setTitle('Add Series');
        },

        series: function (action, query) {
            NzbDrone.mainRegion.show(new NzbDrone.Series.SeriesCollectionView(this, action, query));
            this.setTitle('NzbDrone');

        },

        notFound: function () {
            this.setTitle('Not Found');
        },

        setTitle: function(title)
        {
            $('#title-region').html(title);

            if(title.toLocaleLowerCase() === 'nzbdrone')
            {
                window.document.title = 'NzbDrone';
            }
            else
            {
                window.document.title = title + ' - NzbDrone';
            }
        }
    });


    NzbDrone.addInitializer(function () {

        NzbDrone.addRegions({modalRegion: modalRegion});

        NzbDrone.vent.on(NzbDrone.Events.OpenModalDialog, function (options) {
            console.log('opening modal dialog ' + options.view.template );
            NzbDrone.modalRegion.show(options.view);
        });

        NzbDrone.vent.on(NzbDrone.Events.CloseModalDialog, function () {
            console.log('closing modal dialog');
            NzbDrone.modalRegion.close();
        });

    });

    return new controller();

});

