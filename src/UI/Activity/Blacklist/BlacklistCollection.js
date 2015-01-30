'use strict';
define(
    [
        'Activity/Blacklist/BlacklistModel',
        'backbone.pageable',
        'Mixins/AsSortedCollection',
        'Mixins/AsPersistedStateCollection'
    ], function (BlacklistModel, PageableCollection, AsSortedCollection, AsPersistedStateCollection) {
        var Collection = PageableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/blacklist',
            model: BlacklistModel,

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

            sortMappings: {
                'series'   : { sortKey: 'series.sortTitle' }
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

        Collection = AsSortedCollection.call(Collection);
        return AsPersistedStateCollection.call(Collection);
    });
