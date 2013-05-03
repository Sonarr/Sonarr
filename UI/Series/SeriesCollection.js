"use strict";
define(['app', 'Series/SeriesModel'], function () {
    NzbDrone.Series.SeriesCollection = Backbone.PageableCollection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/series',
        model: NzbDrone.Series.SeriesModel,

        mode: 'client',

        state: {
            sortKey: "title",
            order: -1,
            pageSize: 1000000
        },

        queryParams: {
            sortKey: null,
            order: null
        }
    });
});
