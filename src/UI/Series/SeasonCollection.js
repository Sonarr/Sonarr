﻿'use strict';
define(
    [
        'backbone',
        'Series/SeasonModel'
    ], function (Backbone, SeasonModel) {
        return Backbone.Collection.extend({
            model: SeasonModel,

            comparator: function (season) {
                return -season.get('seasonNumber');
            }
        });
    });
