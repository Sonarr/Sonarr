'use strict';
define(
    [
        'vent',
        'underscore',
        'backbone',
        'signalR'
    ], function (vent, _, Backbone) {

        _.extend(Backbone.Collection.prototype, {
            bindSignalR: function () {

                var collection = this;

                var processMessage = function (options) {

                    if (options.action === 'sync') {
                        console.log('sync received, refetching collection');
                        collection.fetch();

                        return;
                    }

                    var model = new collection.model(options.resource, {parse: true});
                    collection.add(model, {merge: true});
                    console.log(options.action + ': {0}}'.format(options.resource));
                };

                collection.listenTo(vent, 'server:' + collection.url.replace('/api/', ''), processMessage);

                return this;
            },

            unbindSignalR: function () {

            }});
    });


