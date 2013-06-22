'use strict';
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Series.EpisodeCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/episodes',
        model: NzbDrone.Series.EpisodeModel,

        bySeason: function (season) {
            var filtered = this.filter(function (episode) {
                return episode.get('seasonNumber') === season;
            });

            return new NzbDrone.Series.EpisodeCollection(filtered);
        }
    });


    return   NzbDrone.Series.EpisodeCollection;
});
