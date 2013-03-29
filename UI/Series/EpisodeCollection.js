define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Series.EpisodeCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/episodes',
        model: NzbDrone.Series.EpisodeModel
    });
});