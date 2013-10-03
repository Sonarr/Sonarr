'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel',
        'Series/EpisodeModel'
    ], function (Backbone, SeriesModel, EpisodeModel) {
        return Backbone.Model.extend({
            parse: function (model) {
                model.series = new SeriesModel(model.series);
                model.episode = new EpisodeModel(model.episode);
                model.episode.set('series', model.series);
                return model;
            }
        });
    });
