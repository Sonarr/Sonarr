﻿'use strict';

define(
    [
        'marionette',
        'Settings/DownloadClient/DownloadClientCollection',
        'Settings/DownloadClient/DownloadClientCollectionView',
        'Mixins/AsModelBoundView',
        'Mixins/AutoComplete',
        'bootstrap'
    ], function (Marionette, DownloadClientCollection, DownloadClientCollectionView, AsModelBoundView) {

        var view = Marionette.Layout.extend({
            template : 'Settings/DownloadClient/DownloadClientLayoutTemplate',

            regions: {
                downloadClients: '#x-download-clients-region'
            },

            ui: {
                droneFactory: '.x-path'
            },

            events: {
                'change .x-download-client': 'downloadClientChanged'
            },

            initialize: function () {
                this.downloadClientCollection = new DownloadClientCollection();
                this.downloadClientCollection.fetch();
            },

            onShow: function () {
                this.downloadClients.show(new DownloadClientCollectionView({ collection: this.downloadClientCollection }));
                this.ui.droneFactory.autoComplete('/directories');
            }
        });

        return AsModelBoundView.call(view);
    });