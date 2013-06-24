﻿'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel'
    ], function (Backbone, SeriesModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/series/lookup',
            model: SeriesModel,

            parse: function (response) {

                _.each(response, function (model) {
                    model.id = undefined;
                });

                return response;
            }
        });
    });
