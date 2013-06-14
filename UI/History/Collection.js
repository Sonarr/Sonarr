"use strict";
define(['app', 'History/Model', 'backbone.pageable'], function (App, HistoryModel, PageableCollection) {
    NzbDrone.History.Collection = PageableCollection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/history',
        model     : NzbDrone.History.Model,

        state: {
            pageSize: 15,
            sortKey: "date",
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
