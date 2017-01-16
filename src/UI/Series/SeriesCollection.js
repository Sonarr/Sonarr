var _ = require('underscore');
var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var SeriesModel = require('./SeriesModel');
var ApiData = require('../Shared/ApiData');
var AsFilteredCollection = require('../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../Mixins/AsPersistedStateCollection');
var moment = require('moment');
require('../Mixins/backbone.signalr.mixin');

var Collection = PageableCollection.extend({
    url       : window.NzbDrone.ApiRoot + '/series',
    model     : SeriesModel,
    tableName : 'series',

    state : {
        sortKey            : 'sortTitle',
        order              : -1,
        pageSize           : 100000,
        secondarySortKey   : 'sortTitle',
        secondarySortOrder : -1
    },

    mode : 'client',

    save : function() {
        var self = this;

        var proxy = _.extend(new Backbone.Model(), {
            id : '',

            url : self.url + '/editor',

            toJSON : function() {
                return self.filter(function(model) {
                    return model.edited;
                });
            }
        });

        this.listenTo(proxy, 'sync', function(proxyModel, models) {
            this.add(models, { merge : true });
            this.trigger('save', this);
        });

        return proxy.save();
    },

    filterModes : {
        'all'        : [
            null,
            null
        ],
        'continuing' : [
            'status',
            'continuing'
        ],
        'ended'      : [
            'status',
            'ended'
        ],
        'monitored'  : [
            'monitored',
            true
        ],
        'missing'  : [
            null,
            null,
            function(model) { return model.get('episodeCount') !== model.get('episodeFileCount'); }
        ]
    },

    sortMappings : {
        title : {
            sortKey : 'sortTitle'
        },

        nextAiring : {
            sortValue : function(model, attr, order) {
                var nextAiring = model.get(attr);

                if (nextAiring) {
                    return moment(nextAiring).unix();
                }

                if (order === 1) {
                    return 0;
                }

                return Number.MAX_VALUE;
            }
        },

        percentOfEpisodes : {
            sortValue : function(model, attr) {
                var percentOfEpisodes = model.get(attr);
                var episodeCount = model.get('episodeCount');

                return percentOfEpisodes + episodeCount / 1000000;
            }
        },

        path : {
            sortValue : function(model) {
                var path = model.get('path');

                return path.toLowerCase();
            }
        }
    }
});

Collection = AsFilteredCollection.call(Collection);
Collection = AsSortedCollection.call(Collection);
Collection = AsPersistedStateCollection.call(Collection);

var data = ApiData.get('series');

module.exports = new Collection(data, { full : true }).bindSignalR();
