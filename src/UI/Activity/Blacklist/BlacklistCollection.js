var BlacklistModel = require('./BlacklistModel');
var PageableCollection = require('backbone.pageable');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

var Collection = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/blacklist',
    model : BlacklistModel,

    originalFetch : PageableCollection.prototype.fetch,

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

    initialize : function(options) {
        if (options) {
            if (options.mediaType) {
                this.queryParams.mediaType = options.mediaType;
            }
        }
    },

    fetch : function(options) {
        if (!options) {
            options = {};
        }

        options.data = { mediaType : this.queryParams.mediaType };

        return this.originalFetch.call(this, options);
    },


    sortMappings : {
        'series' : { sortKey : 'series.sortTitle' }
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
Collection = AsSortedCollection.call(Collection);
Collection = AsPersistedStateCollection.call(Collection);

module.exports = Collection;