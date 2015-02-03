var _ = require('underscore');
var EpisodeModel = require('../../Series/EpisodeModel');
var PagableCollection = require('backbone.pageable');
var AsFilteredCollection = require('../../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

module.exports = (function(){
    var Collection = PagableCollection.extend({
        url          : window.NzbDrone.ApiRoot + '/wanted/missing',
        model        : EpisodeModel,
        tableName    : 'wanted.missing',
        state        : {
            pageSize : 15,
            sortKey  : 'airDateUtc',
            order    : 1
        },
        queryParams  : {
            totalPages   : null,
            totalRecords : null,
            pageSize     : 'pageSize',
            sortKey      : 'sortKey',
            order        : 'sortDir',
            directions   : {
                "-1" : 'asc',
                "1"  : 'desc'
            }
        },
        filterModes  : {
            "monitored"   : ['monitored', 'true'],
            "unmonitored" : ['monitored', 'false']
        },
        sortMappings : {"series" : {sortKey : 'series.sortTitle'}},
        parseState   : function(resp){
            return {totalRecords : resp.totalRecords};
        },
        parseRecords : function(resp){
            if(resp) {
                return resp.records;
            }
            return resp;
        }
    });
    Collection = AsFilteredCollection.call(Collection);
    Collection = AsSortedCollection.call(Collection);
    return AsPersistedStateCollection.call(Collection);
}).call(this);