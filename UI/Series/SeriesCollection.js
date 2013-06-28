'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel'
    ], function (Backbone, SeriesModel) {
        var collection = Backbone.Collection.extend({
            url  : window.ApiRoot + '/series',
            model: SeriesModel,

            comparator: function (model) {
                return model.get('title');
            },

            state: {
                sortKey: 'title',
                order  : -1
            }
        });

        return new collection();
    });
