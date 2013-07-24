﻿'use strict';
define(
    [
        'backbone',
        'Series/EpisodeModel'
    ], function (Backbone, EpisodeModel) {
        return  Backbone.Collection.extend({
            url  : window.ApiRoot + '/calendar',
            model: EpisodeModel,

            comparator: function (model) {
                var date = new Date(model.get('airDateUtc'));
                var time = date.getTime();
                return time;
            }
        });
    });
