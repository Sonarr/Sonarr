var _ = require('underscore');
var PageableCollection = require('backbone.pageable');
var QueueModel = require('./QueueModel');
var FormatHelpers = require('../../Shared/FormatHelpers');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');

require('../../Mixins/backbone.signalr.mixin');

var QueueCollection = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/queue',
    model : QueueModel,

    state : {
        pageSize : 15
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
        }
    }
});

QueueCollection = AsSortedCollection.call(QueueCollection);

var collection = new QueueCollection().bindSignalR();
collection.fetch();


module.exports = collection;