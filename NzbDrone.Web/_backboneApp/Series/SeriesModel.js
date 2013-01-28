NzbDrone.Series.SeriesModel = Backbone.Model.extend({
    url: NzbDrone.Constants.ApiRoot + '/series'
    
});


NzbDrone.Series.SeriesCollection = Backbone.Collection.extend({
    model:  NzbDrone.Series.SeriesModel,
    url: NzbDrone.Constants.ApiRoot + '/series',
});
