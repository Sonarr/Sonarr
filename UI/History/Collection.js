'use strict';
define(
    [
        'History/Model',
        'backbone.pageable'
    ], function (HistoryModel, PageableCollection) {
        return PageableCollection.extend({
            url  : window.ApiRoot + '/history',
            model: HistoryModel,

            state: {
                pageSize: 15,
                sortKey : 'date',
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

            parseState: function (resp) {
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
