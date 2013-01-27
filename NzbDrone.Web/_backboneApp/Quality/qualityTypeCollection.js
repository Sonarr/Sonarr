NzbDrone.Quality.QualityTypeCollection = Backbone.Collection.extend({
    model: NzbDrone.Quality.QualityTypeModel,
    url: NzbDrone.Constants.ApiRoot + '/qualitytypes'
});