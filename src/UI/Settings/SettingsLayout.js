'use strict';
define(
    [
        'jquery',
        'underscore',
        'vent',
        'marionette',
        'backbone',
        'Settings/General/GeneralSettingsModel',
        'Settings/MediaManagement/Naming/NamingModel',
        'Settings/MediaManagement/MediaManagementLayout',
        'Settings/MediaManagement/MediaManagementSettingsModel',
        'Settings/Quality/QualityLayout',
        'Settings/Indexers/IndexerLayout',
        'Settings/Indexers/Collection',
        'Settings/Indexers/IndexerSettingsModel',
        'Settings/DownloadClient/DownloadClientLayout',
        'Settings/DownloadClient/DownloadClientSettingsModel',
        'Settings/Notifications/CollectionView',
        'Settings/Notifications/Collection',
        'Settings/Metadata/MetadataLayout',
        'Settings/General/GeneralView',
        'Shared/LoadingView',
        'Config'
    ], function ($,
                 _,
                 vent,
                 Marionette,
                 Backbone,
                 GeneralSettingsModel,
                 NamingModel,
                 MediaManagementLayout,
                 MediaManagementSettingsModel,
                 QualityLayout,
                 IndexerLayout,
                 IndexerCollection,
                 IndexerSettingsModel,
                 DownloadClientLayout,
                 DownloadClientSettingsModel,
                 NotificationCollectionView,
                 NotificationCollection,
                 MetadataLayout,
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
                metadata        : '#metadata',
                general         : '#general',
                loading         : '#loading-region'
            },

            ui: {
                mediaManagementTab : '.x-media-management-tab',
                qualityTab         : '.x-quality-tab',
                indexersTab        : '.x-indexers-tab',
                downloadClientTab  : '.x-download-client-tab',
                notificationsTab   : '.x-notifications-tab',
                metadataTab        : '.x-metadata-tab',
                generalTab         : '.x-general-tab',
                advancedSettings   : '.x-advanced-settings'
            },

            events: {
                'click .x-media-management-tab' : '_showMediaManagement',
                'click .x-quality-tab'          : '_showQuality',
                'click .x-indexers-tab'         : '_showIndexers',
                'click .x-download-client-tab'  : '_showDownloadClient',
                'click .x-notifications-tab'    : '_showNotifications',
                'click .x-metadata-tab'         : '_showMetadata',
                'click .x-general-tab'          : '_showGeneral',
                'click .x-save-settings'        : '_save',
                'change .x-advanced-settings'   : '_toggleAdvancedSettings'
            },

            initialize: function (options) {
                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onRender: function () {
                this.loading.show(new LoadingView());
                var self = this;

                this.mediaManagementSettings = new MediaManagementSettingsModel();
                this.namingSettings = new NamingModel();
                this.indexerSettings = new IndexerSettingsModel();
                this.indexerCollection = new IndexerCollection();
                this.downloadClientSettings = new DownloadClientSettingsModel();
                this.notificationCollection = new NotificationCollection();
                this.generalSettings = new GeneralSettingsModel();

                Backbone.$.when(
                        this.mediaManagementSettings.fetch(),
                        this.namingSettings.fetch(),
                        this.indexerSettings.fetch(),
                        this.indexerCollection.fetch(),
                        this.downloadClientSettings.fetch(),
                        this.notificationCollection.fetch(),
                        this.generalSettings.fetch()
                    ).done(function () {
                        if(!self.isClosed)
                        {
                        self.loading.$el.hide();
                        self.mediaManagement.show(new MediaManagementLayout({ settings: self.mediaManagementSettings, namingSettings: self.namingSettings }));
                        self.quality.show(new QualityLayout());
                        self.indexers.show(new IndexerLayout({ settings: self.indexerSettings, indexersCollection: self.indexerCollection }));
                        self.downloadClient.show(new DownloadClientLayout({ model: self.downloadClientSettings }));
                        self.notifications.show(new NotificationCollectionView({ collection: self.notificationCollection }));
                        self.metadata.show(new MetadataLayout());
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
                    case 'metadata':
                        this._showMetadata();
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

            _showMetadata: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.metadataTab.tab('show');
                this._navigate('settings/metadata');
            },

            _showGeneral: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.generalTab.tab('show');
                this._navigate('settings/general');
            },

            _navigate:function(route){
                Backbone.history.navigate(route, { trigger: false, replace: true });
            },

            _save: function () {
                vent.trigger(vent.Commands.SaveSettings);
            },

            _setAdvancedSettingsState: function () {
                var checked = Config.getValueBoolean(Config.Keys.AdvancedSettings);
                this.ui.advancedSettings.prop('checked', checked);

                if (checked) {
                    $('body').addClass('show-advanced-settings');
                }
            },

            _toggleAdvancedSettings: function () {
                var checked = this.ui.advancedSettings.prop('checked');
                Config.setValue(Config.Keys.AdvancedSettings, checked);

                if (checked) {
                    $('body').addClass('show-advanced-settings');
                }

                else {
                    $('body').removeClass('show-advanced-settings');
                }
            }
        });
    });

