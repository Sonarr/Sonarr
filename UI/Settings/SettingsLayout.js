﻿'use strict';
define(
    [
        'app',
        'marionette',
        'Settings/SettingsModel',
        'Settings/General/GeneralSettingsModel',
        'Settings/Naming/NamingModel',
        'Settings/Naming/NamingView',
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
                 NamingView,
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
                naming        : '#naming',
                quality       : '#quality',
                indexers      : '#indexers',
                downloadClient: '#download-client',
                notifications : '#notifications',
                general       : '#general',
                misc          : '#misc',
                loading       : '#loading-region'
            },

            ui: {
                namingTab        : '.x-naming-tab',
                qualityTab       : '.x-quality-tab',
                indexersTab      : '.x-indexers-tab',
                downloadClientTab: '.x-download-client-tab',
                notificationsTab : '.x-notifications-tab',
                generalTab       : '.x-general-tab',
                miscTab          : '.x-misc-tab'
            },

            events: {
                'click .x-naming-tab'         : 'showNaming',
                'click .x-quality-tab'        : 'showQuality',
                'click .x-indexers-tab'       : 'showIndexers',
                'click .x-download-client-tab': 'showDownloadClient',
                'click .x-notifications-tab'  : 'showNotifications',
                'click .x-general-tab'        : 'showGeneral',
                'click .x-misc-tab'           : 'showMisc',
                'click .x-save-settings'      : 'save'
            },

            showNaming: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.namingTab.tab('show');
                this._navigate('settings/naming');
            },

            showQuality: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.qualityTab.tab('show');
                this._navigate('settings/quality');
            },

            showIndexers: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.indexersTab.tab('show');
                this._navigate('settings/indexers');
            },

            showDownloadClient: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.downloadClientTab.tab('show');
                this._navigate('settings/downloadclient');
            },

            showNotifications: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.notificationsTab.tab('show');
                this._navigate('settings/notifications');
            },

            showGeneral: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.generalTab.tab('show');
                this._navigate('settings/general');
            },

            showMisc: function (e) {
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
                    self.naming.show(new NamingView());
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
                        this.showQuality();
                        break;
                    case 'indexers':
                        this.showIndexers();
                        break;
                    case 'downloadclient':
                        this.showDownloadClient();
                        break;
                    case 'notifications':
                        this.showNotifications();
                        break;
                    case 'general':
                        this.showGeneral();
                        break;
                    case 'misc':
                        this.showMisc();
                        break;
                    default:
                        this.showNaming();
                }
            },

            save: function () {
                App.vent.trigger(App.Commands.SaveSettings);
            }
        });
    });

