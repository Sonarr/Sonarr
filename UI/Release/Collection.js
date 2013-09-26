﻿'use strict';
define(
    [
        'backbone',
        'Release/Model'
    ], function (Backbone, ReleaseModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/release',
            model: ReleaseModel,

            state: {
                pageSize: 2000
            },

            fetchEpisodeReleases: function (episodeId) {
                return this.fetch({  data: { episodeId: episodeId  }});
            }
        });
    });
