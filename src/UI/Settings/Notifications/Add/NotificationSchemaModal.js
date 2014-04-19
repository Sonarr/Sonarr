'use strict';

define([
    'AppLayout',
    'Settings/Notifications/NotificationCollection',
    'Settings/Notifications/Add/NotificationAddCollectionView'
], function (AppLayout, SchemaCollection, AddCollectionView) {
    return ({

        open: function (collection) {
            var schemaCollection = new SchemaCollection();
            var originalUrl = schemaCollection.url;
            schemaCollection.url = schemaCollection.url + '/schema';
            schemaCollection.fetch();
            schemaCollection.url = originalUrl;

            var view = new AddCollectionView({ collection: schemaCollection, targetCollection: collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
