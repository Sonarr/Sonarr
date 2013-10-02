'use strict';
define(
    [
        'backbone',
        'History/Queue/QueueModel',
        'Mixins/backbone.signalr.mixin'
    ], function (Backbone, QueueModel) {
        var QueueCollection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/queue',
            model: QueueModel
        });

        var collection = new QueueCollection().bindSignalR();
        collection.fetch();

        return collection;
    });
