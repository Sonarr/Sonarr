define(['app', 'Quality/QualityProfileModel'], function () {

    NzbDrone.Quality.QualityProfileCollection = Backbone.Collection.extend({
        model: NzbDrone.Quality.QualityProfileModel,
        url: NzbDrone.Constants.ApiRoot + '/qualityprofiles'
    });

});
