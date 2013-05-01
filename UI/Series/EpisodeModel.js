"use strict";
define(['app'], function () {
    NzbDrone.Series.EpisodeModel = Backbone.Model.extend({

        mutators: {
            bestDateString     : function () {
                return bestDateString(this.get('airDate'));
            },
            paddedEpisodeNumber: function () {
                return this.get('episodeNumber');
            }
        },

        defaults: {
            seasonNumber: 0
        }
    });
});
