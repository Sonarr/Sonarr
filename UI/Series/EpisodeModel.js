'use strict';
define(
    [
        'backbone',
        'moment',
        'Series/SeriesModel',
        'Series/EpisodeFileModel'
    ], function (Backbone, Moment, SeriesModel, EpisodeFileModel) {
        return Backbone.Model.extend({

            initialize: function () {
                if (this.has('series')) {
                    var start = Moment(this.get('airDateUtc'));
                    var runtime = this.get('series').get('runtime');

                    this.set('end', start.add('minutes', runtime));
                }
            },

            parse: function (model) {
                model.series = new SeriesModel(model.series);

                if (model.episodeFile) {
                    model.episodeFile = new EpisodeFileModel(model.episodeFile);
                }

                return model;
            },

            toJSON: function () {
                var json = _.clone(this.attributes);

                if (this.has('series')) {
                    json.series = this.get('series').toJSON();
                }
                return json;
            },

            defaults: {
                seasonNumber: 0,
                status      : 0
            }
        });
    });
