'use strict';
define(
    [
        'backbone',
        'Health/HealthModel',
        'Mixins/backbone.signalr.mixin'
    ], function (Backbone, HealthModel) {
        var Collection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/health',
            model: HealthModel
        });

        var collection = new Collection().bindSignalR();
        collection.fetch();
        return collection;
    });
