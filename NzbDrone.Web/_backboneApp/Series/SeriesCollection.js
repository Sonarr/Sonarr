/// <reference path="../app.js" />
/// <reference path="SeriesModel.js" />

NzbDrone.Series.SeriesCollection = Backbone.Collection.extend({
    url: NzbDrone.Constants.ApiRoot + '/series',
    model: NzbDrone.Series.SeriesModel,
});