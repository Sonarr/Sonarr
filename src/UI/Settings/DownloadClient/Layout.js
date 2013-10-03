﻿'use strict';

define(
    [
        'marionette',
        'Settings/DownloadClient/SabView',
        'Settings/DownloadClient/BlackholeView',
        'Settings/DownloadClient/PneumaticView',
        'Settings/DownloadClient/NzbgetView',
        'Mixins/AsModelBoundView',
        'Mixins/AutoComplete',
        'bootstrap'
    ], function (Marionette, SabView, BlackholeView, PneumaticView, NzbgetView, AsModelBoundView) {

        var view = Marionette.Layout.extend({
            template : 'Settings/DownloadClient/LayoutTemplate',

            regions: {
                downloadClient: '#download-client-settings-region'
            },

            ui: {
                downloadClientSelect: '.x-download-client',
                downloadedEpisodesFolder: '.x-path'
            },

            events: {
                'change .x-download-client': 'downloadClientChanged'
            },

            onShow: function () {
                this.sabView = new SabView({ model: this.model});
                this.blackholeView = new BlackholeView({ model: this.model});
                this.pneumaticView = new PneumaticView({ model: this.model});
                this.nzbgetView = new NzbgetView({ model: this.model});

                this.ui.downloadedEpisodesFolder.autoComplete('/directories');

                var client = this.model.get('downloadClient');
                this.refreshUIVisibility(client);
            },

            downloadClientChanged: function () {
                var clientId = this.ui.downloadClientSelect.val();
                this.refreshUIVisibility(clientId);
            },

            refreshUIVisibility: function (clientId) {

                if (!clientId) {
                    clientId = 'sabnzbd';
                }

                switch (clientId.toString()) {
                    case 'sabnzbd':
                        this.downloadClient.show(this.sabView);
                        break;

                    case 'blackhole':
                        this.downloadClient.show(this.blackholeView);
                        break;

                    case 'pneumatic':
                        this.downloadClient.show(this.pneumaticView);
                        break;

                    case 'nzbget':
                        this.downloadClient.show(this.nzbgetView);
                        break;

                    default :
                        throw 'unknown download client id' + clientId;
                }
            }
        });

        return AsModelBoundView.call(view);
    });