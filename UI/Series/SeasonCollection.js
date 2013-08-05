﻿'use strict';
define(
    [
        'backbone',
        'Series/SeasonModel'
    ], function (Backbone, SeasonModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/season',
            model: SeasonModel,

            comparator: function (season) {
                return -season.get('seasonNumber');
            },

            bySeries: function (series) {
                var filtered = this.filter(function (season) {
                    return season.get('seriesId') === series;
                });

                var SeasonCollection = require('Series/SeasonCollection');

                return new SeasonCollection(filtered);
            }
        });
    });
