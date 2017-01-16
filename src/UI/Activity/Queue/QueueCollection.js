var _ = require('underscore');
var PageableCollection = require('backbone.pageable');
//var PageableCollection = require('../../Shared/Grid/SonarrPageableCollection');
var QueueModel = require('./QueueModel');
var FormatHelpers = require('../../Shared/FormatHelpers');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPageableCollection = require('../../Mixins/AsPageableCollection');
var moment = require('moment');

require('../../Mixins/backbone.signalr.mixin');

var QueueCollection = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/queue',
    model : QueueModel,

    state : {
        pageSize : 15,
        sortKey: 'timeleft'
    },

    mode : 'client',

    findEpisode : function(episodeId) {
        return _.find(this.fullCollection.models, function(queueModel) {
            return queueModel.get('episode').id === episodeId;
        });
    },

    sortMappings : {
        series : {
            sortValue : function(model, attr) {
                var series = model.get(attr);

                return series.get('sortTitle');
            }
        },

        episode : {
            sortValue : function(model, attr) {
                var episode = model.get('episode');

                return FormatHelpers.pad(episode.get('seasonNumber'), 4) + FormatHelpers.pad(episode.get('episodeNumber'), 4);
            }
        },

        episodeTitle : {
            sortValue : function(model, attr) {
                var episode = model.get('episode');

                return episode.get('title');
            }
        },

        timeleft : {
            sortValue : function(model, attr) {
                var eta = model.get('estimatedCompletionTime');

                if (eta) {
                    return moment(eta).unix();
                }

                return Number.MAX_VALUE;
            }
        },

        sizeleft : {
            sortValue : function(model, attr) {
                var size = model.get('size');
                var sizeleft = model.get('sizeleft');

                if (size && sizeleft) {
                    return sizeleft / size;
                }

                return 0;
            }
        }
    }
});

QueueCollection = AsSortedCollection.call(QueueCollection);
QueueCollection = AsPageableCollection.call(QueueCollection);

var collection = new QueueCollection().bindSignalR();
collection.fetch();

module.exports = collection;