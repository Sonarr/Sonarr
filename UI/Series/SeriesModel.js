'use strict';
define(
    [
        'backbone',

        'underscore'
    ], function (Backbone, _) {
        return Backbone.Model.extend({

            urlRoot: ApiRoot + '/series',

            defaults: {
                episodeFileCount: 0,
                episodeCount    : 0,
                isExisting      : false,
                status          : 0
            },

            setSeasonMonitored: function (seasonNumber) {
                _.each(this.get('seasons'), function (season) {
                    if (season.seasonNumber === seasonNumber) {
                        season.monitored = !season.monitored;
                    }
                });
            },

            setSeasonPass: function (seasonNumber) {
                _.each(this.get('seasons'), function (season) {
                    if (season.seasonNumber >= seasonNumber) {
                        season.monitored = true;
                    }
                    else {
                        season.monitored = false;
                    }
                });
            }
        });
    });
