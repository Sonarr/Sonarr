var _ = require('underscore');
var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var QueueModel = require('./QueueModel');
require('../../Mixins/backbone.signalr.mixin');

module.exports = (function(){
    var QueueCollection = PageableCollection.extend({
        url         : window.NzbDrone.ApiRoot + '/queue',
        model       : QueueModel,
        state       : {pageSize : 15},
        mode        : 'client',
        findEpisode : function(episodeId){
            return _.find(this.fullCollection.models, function(queueModel){
                return queueModel.get('episode').id === episodeId;
            });
        }
    });
    var collection = new QueueCollection().bindSignalR();
    collection.fetch();
    return collection;
}).call(this);