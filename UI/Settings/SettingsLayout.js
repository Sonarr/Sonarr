﻿'use strict';
define(
    [
        'app',
        'marionette',
        'Settings/SettingsModel',
        'Settings/General/GeneralSettingsModel',
        'Settings/MediaManagement/Naming/Model',
        'Settings/MediaManagement/Layout',
        'Settings/Quality/QualityLayout',
        'Settings/Indexers/CollectionView',
        'Settings/Indexers/Collection',
        'Settings/DownloadClient/Layout',
        'Settings/Notifications/CollectionView',
        'Settings/Notifications/Collection',
        'Settings/General/GeneralView',
        'Settings/Misc/MiscView',
        'Shared/LoadingView'
    ], function (App,
                 Marionette,
                 SettingsModel,
                 GeneralSettingsModel,
                 NamingModel,
                 MediaManagementLayout,
                 QualityLayout,
                 IndexerCollectionView,
                 IndexerCollection,
                 DownloadClientLayout,
                 NotificationCollectionView,
                 NotificationCollection,
                 GeneralView,
                 MiscView,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'Settings/SettingsLayoutTemplate',

            regions: {
                mediaManagement : '#media-management',
                quality         : '#quality',
                indexers        : '#indexers',
                downloadClient  : '#download-client',
                notifications   : '#notifications',
                general         : '#general',
                misc            : '#misc',
                loading         : '#loading-region'
            },

            ui: {
                mediaManagementTab : '.x-media-management-tab',
                qualityTab         : '.x-quality-tab',
                indexersTab        : '.x-indexers-tab',
                downloadClientTab  : '.x-download-client-tab',
                notificationsTab   : '.x-notifications-tab',
                generalTab         : '.x-general-tab',
                miscTab            : '.x-misc-tab'
            },

            events: {
                'click .x-media-management-tab' : '_showMediaManagement',
                'click .x-quality-tab'          : '_showQuality',
                'click .x-indexers-tab'         : '_showIndexers',
                'click .x-download-client-tab'  : '_showDownloadClient',
                'click .x-notifications-tab'    : '_showNotifications',
                'click .x-general-tab'          : '_showGeneral',
                'click .x-misc-tab'             : '_showMisc',
                'click .x-save-settings'        : '_save'
            },

            _showMediaManagement: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.mediaManagementTab.tab('show');
                this._navigate('settings/mediamanagement');
            },

            _showQuality: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.qualityTab.tab('show');
                this._navigate('settings/quality');
            },

            _showIndexers: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.indexersTab.tab('show');
                this._navigate('settings/indexers');
            },

            _showDownloadClient: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.downloadClientTab.tab('show');
                this._navigate('settings/downloadclient');
            },

            _showNotifications: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.notificationsTab.tab('show');
                this._navigate('settings/notifications');
            },

            _showGeneral: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.generalTab.tab('show');
                this._navigate('settings/general');
            },

            _showMisc: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.miscTab.tab('show');
                this._navigate('settings/misc');
            },

            _navigate:function(route){
                require(['Router'], function(){
                   App.Router.navigate(route);
                });
            },

            initialize: function (options) {
                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onRender: function () {
                this.loading.show(new LoadingView());
                var self = this;

                this.settings = new SettingsModel();
                this.generalSettings = new GeneralSettingsModel();
                this.namingSettings = new NamingModel();
                this.indexerSettings = new IndexerCollection();
                this.notificationSettings = new NotificationCollection();

                $.when(this.settings.fetch(),
                       this.generalSettings.fetch(),
                       this.namingSettings.fetch(),
                       this.indexerSettings.fetch(),
                       this.notificationSettings.fetch()
                      ).done(function () {
                    self.loading.$el.hide();
                    self.mediaManagement.show(new MediaManagementLayout({ settings: self.settings, namingSettings: self.namingSettings }));
                    self.quality.show(new QualityLayout({settings: self.settings}));
                    self.indexers.show(new IndexerCollectionView({collection: self.indexerSettings}));
                    self.downloadClient.show(new DownloadClientLayout({model: self.settings}));
                    self.notifications.show(new NotificationCollectionView({collection: self.notificationSettings}));
                    self.general.show(new GeneralView({model: self.generalSettings}));
                    self.misc.show(new MiscView({model: self.settings}));
                });
            },

            onShow: function () {
                switch (this.action) {
                    case 'quality':
                        this._showQuality();
                        break;
                    case 'indexers':
                        this._showIndexers();
                        break;
                    case 'downloadclient':
                        this._showDownloadClient();
                        break;
                    case 'notifications':
                        this._showNotifications();
                        break;
                    case 'general':
                        this._showGeneral();
                        break;
                    case 'misc':
                        this._showMisc();
                        break;
                    default:
                        this._showMediaManagement();
                }
            },

            _save: function () {
                App.vent.trigger(App.Commands.SaveSettings);
            }
        });
    });

