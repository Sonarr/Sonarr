"use strict";
define(
    [
        'App',
        'backbone',
        'Series/SeriesModel'
    ], function (App, Backbone, SeriesModel) {
        return Backbone.Collection.extend({
            url  : Constants.ApiRoot + '/series/lookup',
            model: SeriesModel,

            parse: function (response) {

                _.each(response, function (model) {
                    model.id = undefined;
                });

                return response;
            }
        });
    });
