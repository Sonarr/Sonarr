'use strict';
define(
    [
        'History/HistoryModel',
        'backbone.pageable'
    ], function (HistoryModel, PageableCollection) {
        return PageableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/history',
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

            initialize: function (options) {
                delete this.queryParams.episodeId;

                if (options) {
                    if (options.episodeId) {
                        this.queryParams.episodeId = options.episodeId;
                    }
                }
            },

            parseState: function (resp) {
                return { totalRecords: resp.totalRecords };
            },

            parseRecords: function (resp) {
                if (resp) {
                    return resp.records;
                }

                return resp;
            }
        });
    });
