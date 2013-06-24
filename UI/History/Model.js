﻿'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel',
        'Series/EpisodeModel'
    ], function (Backbone, SeriesModel, EpisodeModel) {
        return Backbone.Model.extend({
            mutators: {
                seasonNumber: function () {
                    return this.get('episode').seasonNumber;
                },

                paddedEpisodeNumber: function () {
                    return this.get('episode').episodeNumber.pad(2);
                }
            },

            parse: function (model) {
                model.series = new SeriesModel(model.series);
                model.episode = new EpisodeModel(model.episode);
                return model;
            }
        });
    });
