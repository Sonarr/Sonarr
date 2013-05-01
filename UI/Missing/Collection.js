"use strict";
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Missing.Collection = Backbone.PageableCollection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/missing',
        model     : NzbDrone.Series.EpisodeModel,

        comparator: function (model) {
            return model.get('airDate');
        },

        state: {
            pageSize: 10,
            sortKey: "airDate",
            order: 1
        },

        queryParams: {
            totalPages: null,
            totalRecords: null,
            pageSize: 'pageSize',
            sortKey: "sortBy",
            order: "direction",
            directions: {
                "-1": "asc",
                "1": "desc"
            }
        }
    });
});