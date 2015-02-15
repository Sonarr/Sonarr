var PagableCollection = require('backbone.pageable');
var LogsModel = require('./LogsModel');
var AsFilteredCollection = require('../../Mixins/AsFilteredCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

var collection = PagableCollection.extend({
    url       : window.NzbDrone.ApiRoot + '/log',
    model     : LogsModel,
    tableName : 'logs',

    state : {
        pageSize : 50,
        sortKey  : 'time',
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

    // Filter Modes
    filterModes : {
        "all"   : [
            null,
            null
        ],
        "info"  : [
            'level',
            'Info'
        ],
        "warn"  : [
            'level',
            'Warn'
        ],
        "error" : [
            'level',
            'Error'
        ]
    },

    parseState : function(resp, queryParams, state) {
        return { totalRecords : resp.totalRecords };
    },

    parseRecords : function(resp) {
        if (resp) {
            return resp.records;
        }

        return resp;
    }
});

collection = AsFilteredCollection.apply(collection);

module.exports = AsPersistedStateCollection.apply(collection);