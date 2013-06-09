"use strict";
define(['app','Series/SeriesModel', 'Series/EpisodeModel'], function () {
    NzbDrone.History.Model = Backbone.Model.extend({
        mutators: {
            seasonNumber: function () {
                return this.get('episode').seasonNumber;
            },

            paddedEpisodeNumber: function () {
                return this.get('episode').episodeNumber.pad(2);
            }
        },

        parse: function (model) {
            model.series = new NzbDrone.Series.SeriesModel(model.series);
            model.episode = new NzbDrone.Series.EpisodeModel(model.episode);
            return model;
        }


    });
});
