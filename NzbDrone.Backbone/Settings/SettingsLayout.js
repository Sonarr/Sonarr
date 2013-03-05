define([
    'app',
    'Quality/QualityProfileCollection',
    'Settings/Naming/NamingView',
    'Settings/Quality/QualityView',
    'Settings/Indexers/IndexersView',
    'Settings/DownloadClient/DownloadClientView',
    'Settings/Notifications/NotificationsView',
    'Settings/System/SystemView',
    'Settings/Misc/MiscView'
],
    function (app, qualityProfileCollection) {
        NzbDrone.Settings.SettingsLayout = Backbone.Marionette.Layout.extend({
            template: 'Settings/SettingsLayoutTemplate',

            regions: {
                naming: '#naming',
                quality: '#quality',
                indexers: '#indexers',
                downloadClient: '#download-client',
                notifications: '#notifications',
                system: '#system',
                misc: '#misc'
            },

            ui: {
                namingTab: '.x-naming-tab',
                qualityTab: '.x-quality-tab',
                indexersTab: '.x-indexers-tab',
                downloadClientTab: '.x-download-client-tab',
                notificationsTab: '.x-notifications-tab',
                systemTab: '.x-system-tab',
                miscTab: '.x-misc-tab'
            },

            events: {
                'click .x-naming-tab': 'showNaming',
                'click .x-quality-tab': 'showQuality',
                'click .x-indexers-tab': 'showIndexers',
                'click .x-download-client-tab': 'showDownloadClient',
                'click .x-notifications-tab': 'showNotifications',
                'click .x-system-tab': 'showSystem',
                'click .x-misc-tab': 'showMisc',
                'click .x-save-settings': 'save'
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

            initialize: function (context, action, query, settings) {
                this.settings = settings;

                if (action) {
                    this.action = action.toLowerCase();
                }

                if (query) {
                    this.query = query.toLowerCase();
                }
            },

            onRender: function () {
                qualityProfileCollection.fetch();

                this.naming.show(new NzbDrone.Settings.Naming.NamingView({model: this.settings}));
                this.quality.show(new NzbDrone.Settings.Quality.QualityView({model: this.settings}));
                this.indexers.show(new NzbDrone.Settings.Indexers.IndexersView({model: this.settings}));
                this.downloadClient.show(new NzbDrone.Settings.DownloadClient.DownloadClientView({model: this.settings}));
                this.notifications.show(new NzbDrone.Settings.Notifications.NotificationsView({model: this.settings}));
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
                this.settings.save();
            }
        });
    });

