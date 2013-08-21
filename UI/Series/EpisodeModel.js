'use strict';
define(
    [
        'backbone',
        'moment',
        'Series/SeriesModel',
        'Series/EpisodeFileModel'
    ], function (Backbone, Moment, SeriesModel, EpisodeFileModel) {
        return Backbone.Model.extend({

            parse: function (model) {

                if (model.episodeFile) {
                    model.episodeFile = new EpisodeFileModel(model.episodeFile);
                }

                return model;
            },

            defaults: {
                seasonNumber: 0,
                status      : 0
            }
        });
    });
