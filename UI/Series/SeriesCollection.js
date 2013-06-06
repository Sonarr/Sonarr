"use strict";
define(['app', 'Series/SeriesModel'], function () {
    NzbDrone.Series.SeriesCollection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/series',
        model: NzbDrone.Series.SeriesModel,

        comparator: function(model) {
            return model.get('title');
        },

        state: {
            sortKey: "title",
            order: -1
        }
    });
});
