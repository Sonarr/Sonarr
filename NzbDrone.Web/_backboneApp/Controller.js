define(['app', 'AddSeries/AddSeriesLayout','Series/SeriesCollectionView'], function () {

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

    return new controller();

});

