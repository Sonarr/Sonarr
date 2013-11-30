'use strict';
define(
    [
        'underscore',
        'backbone',
        'History/Queue/QueueModel',
        'Mixins/backbone.signalr.mixin'
    ], function (_, Backbone, QueueModel) {
        var QueueCollection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/queue',
            model: QueueModel,

            findEpisode: function (episodeId) {
                return _.find(this.models, function (queueModel) {
                    return queueModel.get('episode').id === episodeId;
                });
            }
        });

        var collection = new QueueCollection().bindSignalR();
        collection.fetch();

        return collection;
    });
