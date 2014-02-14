'use strict';
define([
    'AppLayout',
    'Settings/DownloadClient/DownloadClientCollection',
    'Settings/DownloadClient/Add/DownloadClientAddCollectionView'
], function (AppLayout, DownloadClientCollection, DownloadClientAddCollectionView) {
    return ({

        open: function (collection) {
            var schemaCollection = new DownloadClientCollection();
            var originalUrl = schemaCollection.url;
            schemaCollection.url = schemaCollection.url + '/schema';
            schemaCollection.fetch();
            schemaCollection.url = originalUrl;

            var view = new DownloadClientAddCollectionView({ collection: schemaCollection, downloadClientCollection: collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
