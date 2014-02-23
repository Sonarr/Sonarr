'use strict';
define(
    [
        'underscore',
        'backbone',
        'backbone.pageable',
        'History/Queue/QueueModel',
        'Mixins/backbone.signalr.mixin'
    ], function (_, Backbone, PageableCollection, QueueModel) {
        var QueueCollection = PageableCollection.extend({
            url  : window.NzbDrone.ApiRoot + '/queue',
            model: QueueModel,

            state: {
                pageSize: 15
            },

            mode: 'client',

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
