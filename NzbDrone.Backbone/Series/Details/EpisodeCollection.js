define(['app', 'Series/Details/EpisodeModel'], function () {
    NzbDrone.Series.Details.EpisodeCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/episode',
        model: NzbDrone.Series.Details.EpisodeModel
    });
});