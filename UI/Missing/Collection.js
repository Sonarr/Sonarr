"use strict";
define(['app', 'Series/EpisodeModel'], function () {
    NzbDrone.Missing.Collection = Backbone.PageableCollection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/missing',
        model     : NzbDrone.Series.EpisodeModel,

        state: {
            pageSize: 15,
            sortKey: "airDate",
            order: 1
        },

        queryParams: {
            totalPages: null,
            totalRecords: null,
            pageSize: 'pageSize',
            sortKey: "sortKey",
            order: "sortDir",
            directions: {
                "-1": "asc",
                "1": "desc"
            }
        },

        parseState: function (resp, queryParams, state) {
            return {totalRecords: resp.totalRecords};
        },

        parseRecords: function (resp) {
            if (resp) {
                return resp.records;
            }

            return resp;
        }
    });
});