'use strict';
define(
    [
        'History/HistoryModel',
        'backbone.pageable',
        'Mixins/AsFilteredCollection',
        'Mixins/AsPersistedStateCollection'
    ], function (HistoryModel, PageableCollection, AsFilteredCollection, AsPersistedStateCollection) {
        var collection = PageableCollection.extend({
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

            filterModes: {
                'all'      : [null, null],
                'grabbed'  : ['eventType', '1'],
                'imported' : ['eventType', '3'],
                'failed'   : ['eventType', '4']
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

        collection = AsFilteredCollection.call(collection);
        return AsPersistedStateCollection.call(collection);
    });
