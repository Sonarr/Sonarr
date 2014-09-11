﻿'use strict';

define([
    'marionette',
    'Settings/DownloadClient/DownloadClientCollection',
    'Settings/DownloadClient/DownloadClientCollectionView',
    'Settings/DownloadClient/DownloadHandling/DownloadHandlingView',
    'Settings/DownloadClient/DroneFactory/DroneFactoryView',
    'Settings/DownloadClient/RemotePathMapping/RemotePathMappingCollection',
    'Settings/DownloadClient/RemotePathMapping/RemotePathMappingCollectionView'
], function (Marionette, DownloadClientCollection, DownloadClientCollectionView, DownloadHandlingView, DroneFactoryView, RemotePathMappingCollection, RemotePathMappingCollectionView) {

    return Marionette.Layout.extend({
        template : 'Settings/DownloadClient/DownloadClientLayoutTemplate',

        regions: {
            downloadClients        : '#x-download-clients-region',
            downloadHandling       : '#x-download-handling-region',
            droneFactory           : '#x-dronefactory-region',
            remotePathMappings     : '#x-remotepath-mapping-region'
        },

        initialize: function () {
            this.downloadClientsCollection = new DownloadClientCollection();
            this.downloadClientsCollection.fetch();
            this.remotePathMappingCollection = new RemotePathMappingCollection();
            this.remotePathMappingCollection.fetch();
        },

        onShow: function () {
            this.downloadClients.show(new DownloadClientCollectionView({ collection: this.downloadClientsCollection }));
            this.downloadHandling.show(new DownloadHandlingView({ model: this.model }));
            this.droneFactory.show(new DroneFactoryView({ model: this.model }));
            this.remotePathMappings.show(new RemotePathMappingCollectionView({ collection: this.remotePathMappingCollection }));
        }
    });
});