﻿'use strict';

define(
    [
        'marionette',
        'Settings/DownloadClient/DownloadClientCollection',
        'Settings/DownloadClient/DownloadClientCollectionView',
        'Settings/DownloadClient/Options/DownloadClientOptionsView',
        'Settings/DownloadClient/FailedDownloadHandling/FailedDownloadHandlingView'
    ], function (Marionette, DownloadClientCollection, DownloadClientCollectionView, DownloadClientOptionsView, FailedDownloadHandlingView) {

        return Marionette.Layout.extend({
            template : 'Settings/DownloadClient/DownloadClientLayoutTemplate',

            regions: {
                downloadClients        : '#x-download-clients-region',
                downloadClientOptions  : '#x-download-client-options-region',
                failedDownloadHandling : '#x-failed-download-handling-region'
            },

            initialize: function () {
                this.downloadClientCollection = new DownloadClientCollection();
                this.downloadClientCollection.fetch();
            },

            onShow: function () {
                this.downloadClients.show(new DownloadClientCollectionView({ collection: this.downloadClientCollection }));
                this.downloadClientOptions.show(new DownloadClientOptionsView({ model: this.model }));
                this.failedDownloadHandling.show(new FailedDownloadHandlingView({ model: this.model }));
            }
        });
    });