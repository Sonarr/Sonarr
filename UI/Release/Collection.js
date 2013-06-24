﻿'use strict';
define(
    [
        'Release/Model',
        'backbone.pageable'
    ], function (ReleaseModel, PagableCollection) {
        return PagableCollection.extend({
            url  : window.ApiRoot + '/release',
            model: ReleaseModel,

            mode: 'client',

            state: {
                pageSize: 2000
            },

            fetchEpisodeReleases: function (episodeId) {
                return this.fetch({  data: { episodeId: episodeId  }});
            }
        });
    });
