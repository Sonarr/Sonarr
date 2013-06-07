"use strict";
define(['app', 'Release/Model'], function () {
    NzbDrone.Release.Collection = Backbone.PageableCollection.extend({
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
