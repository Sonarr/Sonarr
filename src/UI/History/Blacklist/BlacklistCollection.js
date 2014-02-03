'use strict';
define(
    [
        'History/Blacklist/BlacklistModel',
        'backbone.pageable',
        'Mixins/AsPersistedStateCollection'
    ], function (BlacklistModel, PageableCollection, AsPersistedStateCollection) {
        var collection = PageableCollection.extend({
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

        return AsPersistedStateCollection.apply(collection);
    });
