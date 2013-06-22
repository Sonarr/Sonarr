'use strict';
define(['app', 'Release/Model', 'backbone.pageable'], function (app, SeriesModel, PagableCollection) {
    NzbDrone.Release.Collection = PagableCollection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/release',
        model: NzbDrone.Release.Model,

        mode: 'client',

        state: {
            pageSize: 2000
        },

        fetchEpisodeReleases: function (episodeId) {
            return this.fetch({  data: { episodeId: episodeId  }});
        }
    });
});
