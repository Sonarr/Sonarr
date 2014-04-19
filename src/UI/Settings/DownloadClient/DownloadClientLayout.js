﻿'use strict';

define([
    'marionette',
    'Settings/DownloadClient/DownloadClientCollection',
    'Settings/DownloadClient/DownloadClientCollectionView',
    'Settings/DownloadClient/DroneFactory/DroneFactoryView',
    'Settings/DownloadClient/DownloadHandling/DownloadHandlingView'
], function (Marionette, DownloadClientCollection, CollectionView, DroneFactoryView, DownloadHandlingView) {

    return Marionette.Layout.extend({
        template : 'Settings/DownloadClient/DownloadClientLayoutTemplate',

        regions: {
            downloadClients        : '#x-download-clients-region',
            downloadHandling       : '#x-download-handling-region',
            droneFactory           : '#x-dronefactory-region'
        },

        initialize: function () {
            this.downloadClientsCollection = new DownloadClientCollection();
            this.downloadClientsCollection.fetch();
        },

        onShow: function () {
            this.downloadClients.show(new CollectionView({ collection: this.downloadClientsCollection }));
            this.downloadHandling.show(new DownloadHandlingView({ model: this.model }));
            this.droneFactory.show(new DroneFactoryView({ model: this.model }));
        }
    });
});