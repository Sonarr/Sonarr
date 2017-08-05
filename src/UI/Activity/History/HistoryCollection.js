var HistoryModel = require('./HistoryModel');
var PageableCollection = require('backbone.pageable');
var AsFilteredCollection = require('../../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

var Collection = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/history',
    model : HistoryModel,

    state : {
        pageSize : 15,
        sortKey  : 'date',
        order    : 1
    },

    queryParams : {
        totalPages   : null,
        totalRecords : null,
        pageSize     : 'pageSize',
        sortKey      : 'sortKey',
        order        : 'sortDir',
        directions   : {
            '-1' : 'asc',
            '1'  : 'desc'
        }
    },

    filterModes : {
        'all'      : [
            null,
            null
        ],
        'grabbed'  : [
            'eventType',
            '1'
        ],
        'imported' : [
            'eventType',
            '3'
        ],
        'failed'   : [
            'eventType',
            '4'
        ],
        'deleted'  : [
            'eventType',
            '5'
        ],
        'renamed'  : [
            'eventType',
            '6'
        ]
    },

    sortMappings : {
        'series' : { sortKey : 'series.sortTitle' }
    },

    initialize : function(options) {
        delete this.queryParams.episodeId;

        if (options) {
            if (options.episodeId) {
                this.queryParams.episodeId = options.episodeId;
            }
        }
    },

    parseState : function(resp) {
        return { totalRecords : resp.totalRecords };
    },

    parseRecords : function(resp) {
        if (resp) {
            return resp.records;
        }

        return resp;
    }
});

Collection = AsFilteredCollection.call(Collection);
Collection = AsSortedCollection.call(Collection);
Collection = AsPersistedStateCollection.call(Collection);

module.exports = Collection;
