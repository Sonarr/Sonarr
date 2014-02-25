'use strict';
define(
    [
        'underscore',
        'Series/EpisodeModel',
        'backbone.pageable',
        'Mixins/AsFilteredCollection',
        'Mixins/AsPersistedStateCollection'
    ], function (_, EpisodeModel, PagableCollection, AsFilteredCollection, AsPersistedStateCollection) {
        var collection = PagableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/wanted/missing',
            model: EpisodeModel,
            tableName: 'wanted.missing',

            state: {
                pageSize    : 15,
                sortKey     : 'airDateUtc',
                order       : 1
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
                'monitored'   : ['monitored', 'true'],
                'unmonitored' : ['monitored', 'false']
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

        collection = AsFilteredCollection.call(collection);
        return AsPersistedStateCollection.call(collection);
    });
