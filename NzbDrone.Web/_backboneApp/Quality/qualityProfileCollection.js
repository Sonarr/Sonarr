/// <reference path="../app.js" />
/// <reference path="qualityProfileModel.js" />

NzbDrone.Quality.QualityProfileCollection = Backbone.Collection.extend({
    model: NzbDrone.Quality.QualityProfileModel,
    url: NzbDrone.Constants.ApiRoot + '/qualityprofiles'
});