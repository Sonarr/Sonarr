"use strict";
define([
    'app',
    'marionette',
    'Settings/SettingsModel',
    'Settings/General/GeneralSettingsModel',
    'Settings/Naming/NamingView',
    'Settings/Naming/NamingModel',
    'Settings/Quality/QualityLayout',
    'Settings/Indexers/CollectionView',
    'Settings/Indexers/Collection',
    'Settings/DownloadClient/DownloadClientView',
    'Settings/Notifications/CollectionView',
    'Settings/Notifications/Collection',
    'Settings/General/GeneralView',
    'Settings/Misc/MiscView',
    'Settings/SyncNotification'
],
    function (App,
        Marionette,
        SettingsModel,
        GeneralSettingsModel,
        NamingView,
        NamingModel,
        QualityLayout,
        IndexerCollectionView,
        IndexerCollection,
        DownloadClientView,
        NotificationCollectionView,
        NotificationCollection,
        GeneralView,
        MiscView,
        SyncNotification) {
        return Marionette.Layout.extend({
            template: 'Settings/SettingsLayoutTemplate',

            regions: {
                naming        : '#naming',
                quality       : '#quality',
                indexers      : '#indexers',
                downloadClient: '#download-client',
                notifications : '#notifications',
                general       : '#general',
                misc          : '#misc'
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
                App.Router.navigate('settings/naming');
            },

            showQuality: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.qualityTab.tab('show');
                App.Router.navigate('settings/quality');
            },

            showIndexers: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.indexersTab.tab('show');
                App.Router.navigate('settings/indexers');
            },

            showDownloadClient: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.downloadClientTab.tab('show');
                App.Router.navigate('settings/downloadclient');
            },

            showNotifications: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.notificationsTab.tab('show');
                App.Router.navigate('settings/notifications');
            },

            showGeneral: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.generalTab.tab('show');
                App.Router.navigate('settings/general');
            },

            showMisc: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.miscTab.tab('show');
                App.Router.navigate('settings/misc');
            },

            initialize: function (options) {
                this.settings = new SettingsModel();
                this.settings.fetch();

                this.generalSettings = new GeneralSettingsModel();
                this.generalSettings.fetch();

                this.namingSettings = new NamingModel();
                this.namingSettings.fetch();

                this.indexerSettings = new IndexerCollection();
                this.indexerSettings.fetch();

                this.notificationSettings = new NotificationCollection();
                this.notificationSettings.fetch();

                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onRender: function () {
                this.naming.show(new NamingView());
                this.quality.show(new QualityLayout({settings: this.settings}));
                this.indexers.show(new IndexerCollectionView({collection: this.indexerSettings}));
                this.downloadClient.show(new DownloadClientView({model: this.settings}));
                this.notifications.show(new NotificationCollectionView({collection: this.notificationSettings}));
                this.general.show(new GeneralView({model: this.generalSettings}));
                this.misc.show(new MiscView({model: this.settings}));
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

                this.settings.saveIfChanged(undefined, SyncNotification.callback({
                    successMessage: 'Settings saved',
                    errorMessage  : "Failed to save settings"
                }));
            }
        });
    });

