'use strict';
define(
    [
        'Series/EpisodeModel',
        'backbone.pageable',
        'Mixins/AsPersistedStateCollection'
    ], function (EpisodeModel, PagableCollection, AsPersistedStateCollection) {
        var collection = PagableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/missing',
            model: EpisodeModel,
            tableName: 'missing',

            state: {
                pageSize: 15,
                sortKey : 'airDateUtc',
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

        return AsPersistedStateCollection.call(collection);
    });
