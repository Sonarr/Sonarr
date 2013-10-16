'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel',
        'api!series'
    ], function (Backbone, SeriesModel, SeriesData) {
        var Collection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/series',
            model: SeriesModel,

            comparator: function (model) {
                return model.get('title');
            },

            state: {
                sortKey: 'title',
                order  : -1
            }
        });

        var collection = new Collection(SeriesData);
        return collection;
    });
