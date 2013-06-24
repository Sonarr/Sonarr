﻿'use strict';
define(['backbone.pageable', 'Logs/Model', ], function (PagableCollection, LogsModel) {
    return PagableCollection.extend({
        url  : window.ApiRoot + '/log',
        model: LogsModel,

        state: {
            pageSize: 50,
            sortKey : 'time',
            order   : 1
        },

        queryParams: {
            totalPages  : null,
            totalRecords: null,
            pageSize    : 'pageSize',
            sortKey     : 'sortKey',
            order       : 'sortDir',
            directions  : {
                '-1': 'asc',
                '1' : 'desc'
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
