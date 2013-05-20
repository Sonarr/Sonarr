"use strict";
define([
    'app',
    'Settings/Naming/NamingView',
    'Settings/Quality/QualityLayout',
    'Settings/Indexers/CollectionView',
    'Settings/DownloadClient/DownloadClientView',
    'Settings/Notifications/CollectionView',
    'Settings/System/SystemView',
    'Settings/Misc/MiscView'
],
    function () {
        NzbDrone.Settings.SettingsLayout = Backbone.Marionette.Layout.extend({
            template: 'Settings/SettingsLayoutTemplate',

            regions: {
                naming        : '#naming',
                quality       : '#quality',
                indexers      : '#indexers',
                downloadClient: '#download-client',
                notifications : '#notifications',
                system        : '#system',
                misc          : '#misc'
            },

            ui: {
                namingTab        : '.x-naming-tab',
                qualityTab       : '.x-quality-tab',
                indexersTab      : '.x-indexers-tab',
                downloadClientTab: '.x-download-client-tab',
                notificationsTab : '.x-notifications-tab',
                systemTab        : '.x-system-tab',
                miscTab          : '.x-misc-tab'
            },

            events: {
                'click .x-naming-tab'         : 'showNaming',
                'click .x-quality-tab'        : 'showQuality',
                'click .x-indexers-tab'       : 'showIndexers',
                'click .x-download-client-tab': 'showDownloadClient',
                'click .x-notifications-tab'  : 'showNotifications',
                'click .x-system-tab'         : 'showSystem',
                'click .x-misc-tab'           : 'showMisc',
                'click .x-save-settings'      : 'save'
            },

            showNaming: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.namingTab.tab('show');
                NzbDrone.Router.navigate('settings/naming');
            },

            showQuality: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.qualityTab.tab('show');
                NzbDrone.Router.navigate('settings/quality');
            },

            showIndexers: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.indexersTab.tab('show');
                NzbDrone.Router.navigate('settings/indexers');
            },

            showDownloadClient: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.downloadClientTab.tab('show');
                NzbDrone.Router.navigate('settings/downloadclient');
            },

            showNotifications: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.notificationsTab.tab('show');
                NzbDrone.Router.navigate('settings/notifications');
            },

            showSystem: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.systemTab.tab('show');
                NzbDrone.Router.navigate('settings/system');
            },

            showMisc: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.miscTab.tab('show');
                NzbDrone.Router.navigate('settings/misc');
            },

            initialize: function (options) {
                this.settings = new NzbDrone.Settings.SettingsModel();
                this.settings.fetch();

                this.namingSettings = new NzbDrone.Settings.Naming.NamingModel();
                this.namingSettings.fetch();

                this.indexerSettings = new NzbDrone.Settings.Indexers.Collection();
                this.indexerSettings.fetch();

                this.notificationSettings = new NzbDrone.Settings.Notifications.Collection();
                this.notificationSettings.fetch();

                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onRender: function () {
                this.naming.show(new NzbDrone.Settings.Naming.NamingView());
                this.quality.show(new NzbDrone.Settings.Quality.QualityLayout({settings: this.settings}));
                this.indexers.show(new NzbDrone.Settings.Indexers.CollectionView({collection: this.indexerSettings}));
                this.downloadClient.show(new NzbDrone.Settings.DownloadClient.DownloadClientView({model: this.settings}));
                this.notifications.show(new NzbDrone.Settings.Notifications.CollectionView({collection: this.notificationSettings}));
                this.system.show(new NzbDrone.Settings.System.SystemView({model: this.settings}));
                this.misc.show(new NzbDrone.Settings.Misc.MiscView({model: this.settings}));
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
                    case 'system':
                        this.showSystem();
                        break;
                    case 'misc':
                        this.showMisc();
                        break;
                    default:
                        this.showNaming();
                }
            },

            save: function () {

                NzbDrone.vent.trigger(NzbDrone.Commands.SaveSettings);

                this.settings.save(undefined,
                    {
                        success: function () {
                            window.alert('Saved');
                        },
                        error  : function () {
                            window.alert("couldn't save settings");
                        }
                    });

            }
        })
        ;
    })
;

