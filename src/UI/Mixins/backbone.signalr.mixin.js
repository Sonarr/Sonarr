'use strict';
define(
    [
        'vent',
        'underscore',
        'backbone',
        'signalR'
    ], function (vent, _, Backbone) {

        _.extend(Backbone.Collection.prototype, {
            bindSignalR: function (bindOptions) {

                var collection = this;
                bindOptions = bindOptions || {};

                var processMessage = function (options) {

                    if (options.action === 'sync') {
                        console.log('sync received, refetching collection');
                        collection.fetch();

                        return;
                    }

                    if (options.action === 'deleted') {
                        collection.remove(new collection.model(options.resource, {parse: true}));

                        return;
                    }

                    var model = new collection.model(options.resource, {parse: true});

                    //updateOnly will prevent the collection from adding a new item
                    if (bindOptions.updateOnly && !collection.get(model.get('id'))) {
                        return;
                    }

                    collection.add(model, {merge: true});
                    console.log(options.action + ': {0}}'.format(options.resource));
                };

                collection.listenTo(vent, 'server:' + collection.url.replace('/api/', ''), processMessage);

                return this;
            },

            unbindSignalR: function () {

            }});
    });


