"use strict";
define(['app', 'Logs/Model'], function () {
    NzbDrone.Logs.Collection = Backbone.PageableCollection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/log',
        model     : NzbDrone.Logs.Model,

        state: {
            pageSize: 50,
            sortKey: "time",
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
