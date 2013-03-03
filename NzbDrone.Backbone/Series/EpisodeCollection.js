define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Series.EpisodeCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/episode',
        model: NzbDrone.Series.EpisodeModel
    });
});