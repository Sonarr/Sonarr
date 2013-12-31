'use strict';
define(
    [
        'vent',
        'marionette',
        'backbone',
        'Settings/SettingsModel',
        'Settings/General/GeneralSettingsModel',
        'Settings/MediaManagement/Naming/NamingModel',
        'Settings/MediaManagement/MediaManagementLayout',
        'Settings/Quality/QualityLayout',
        'Settings/Indexers/IndexerLayout',
        'Settings/Indexers/Collection',
        'Settings/DownloadClient/Layout',
        'Settings/Notifications/CollectionView',
        'Settings/Notifications/Collection',
        'Settings/General/GeneralView',
        'Shared/LoadingView',
        'Config'
    ], function (vent,
                 Marionette,
                 Backbone,
                 SettingsModel,
                 GeneralSettingsModel,
                 NamingModel,
                 MediaManagementLayout,
                 QualityLayout,
                 IndexerLayout,
                 IndexerCollection,
                 DownloadClientLayout,
                 NotificationCollectionView,
                 NotificationCollection,
                 GeneralView,
                 LoadingView,
                 Config) {
        return Marionette.Layout.extend({
            template: 'Settings/SettingsLayoutTemplate',

            regions: {
                mediaManagement : '#media-management',
                quality         : '#quality',
                indexers        : '#indexers',
                downloadClient  : '#download-client',
                notifications   : '#notifications',
                general         : '#general',
                loading         : '#loading-region'
            },

            ui: {
                mediaManagementTab : '.x-media-management-tab',
                qualityTab         : '.x-quality-tab',
                indexersTab        : '.x-indexers-tab',
                downloadClientTab  : '.x-download-client-tab',
                notificationsTab   : '.x-notifications-tab',
                generalTab         : '.x-general-tab',
                advancedSettings    : '.x-advanced-settings'
            },

            events: {
                'click .x-media-management-tab' : '_showMediaManagement',
                'click .x-quality-tab'          : '_showQuality',
                'click .x-indexers-tab'         : '_showIndexers',
                'click .x-download-client-tab'  : '_showDownloadClient',
                'click .x-notifications-tab'    : '_showNotifications',
                'click .x-general-tab'          : '_showGeneral',
                'click .x-save-settings'        : '_save',
                'change .x-advanced-settings'     : '_toggleAdvancedSettings'
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

                Backbone.$.when(this.settings.fetch(),
                        this.generalSettings.fetch(),
                        this.namingSettings.fetch(),
                        this.indexerSettings.fetch(),
                        this.notificationSettings.fetch()
                    ).done(function () {
                        if(!self.isClosed)
                        {
                        self.loading.$el.hide();
                        self.mediaManagement.show(new MediaManagementLayout({ settings: self.settings, namingSettings: self.namingSettings }));
                        self.quality.show(new QualityLayout({ settings: self.settings }));
                        self.indexers.show(new IndexerLayout({ settings: self.settings, indexersCollection: self.indexerSettings }));
                        self.downloadClient.show(new DownloadClientLayout({ model: self.settings }));
                        self.notifications.show(new NotificationCollectionView({ collection: self.notificationSettings }));
                        self.general.show(new GeneralView({ model: self.generalSettings }));
                        }
                    });

                this._setAdvancedSettingsState();
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
                    case 'connect':
                        this._showNotifications();
                        break;
                    case 'notifications':
                        this._showNotifications();
                        break;
                    case 'general':
                        this._showGeneral();
                        break;
                    default:
                        this._showMediaManagement();
                }
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
                this._navigate('settings/connect');
            },

            _showGeneral: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.generalTab.tab('show');
                this._navigate('settings/general');
            },

            _navigate:function(route){
                Backbone.history.navigate(route, { trigger: true, replace: true });
            },

            _save: function () {
                vent.trigger(vent.Commands.SaveSettings);
            },

            _setAdvancedSettingsState: function () {
                var checked = Config.getValueBoolean('advancedSettings');
                this.ui.advancedSettings.prop('checked', checked);

                if (checked) {
                    this.$el.addClass('show-advanced-settings');
                }
            },

            _toggleAdvancedSettings: function () {
                var checked = this.ui.advancedSettings.prop('checked');
                Config.setValue('advancedSettings', checked);

                if (checked) {
                    this.$el.addClass('show-advanced-settings');
                }

                else {
                    this.$el.removeClass('show-advanced-settings');
                }
            }
        });
    });

