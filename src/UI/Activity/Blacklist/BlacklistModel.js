'use strict';
define(
    [
        'backbone',
        'Series/SeriesCollection'
    ], function (Backbone, SeriesCollection) {
        return Backbone.Model.extend({

            //Hack to deal with Backbone 1.0's bug
            initialize: function () {
                this.url = function () {
                    return this.collection.url + '/' + this.get('id');
                };
            },

            parse: function (model) {
                model.series = SeriesCollection.get(model.seriesId);
                return model;
            }
        });
    });
