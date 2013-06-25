'use strict';
define(
    [
        'backbone',
        'Series/EpisodeModel'
    ], function (Backbone, EpisodeModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/episodes',
            model: EpisodeModel,

            bySeason: function (season) {
                var filtered = this.filter(function (episode) {
                    return episode.get('seasonNumber') === season;
                });

                var EpisodeCollection = require('Series/EpisodeCollection');

                return new EpisodeCollection(filtered);
            }
        });
    });
