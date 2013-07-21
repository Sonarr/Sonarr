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
            },

            comparator: function (model1, model2) {
                var episode1 = model1.get('episodeNumber');

                var episode2 = model2.get('episodeNumber');

                if (episode1 < episode2){
                    return 1;
                }

                if (episode1 > episode2){
                    return -1;
                }

                return 0;
            },

            state: {
                sortKey: 'episodeNumber',
                order  : -1
            }
        });
    });
