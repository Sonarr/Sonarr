var $ = require('jquery');
var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backbone = require('backbone');
var GeneralSettingsModel = require('./General/GeneralSettingsModel');
var NamingModel = require('./MediaManagement/Naming/NamingModel');
var MediaManagementLayout = require('./MediaManagement/MediaManagementLayout');
var MediaManagementSettingsModel = require('./MediaManagement/MediaManagementSettingsModel');
var ProfileLayout = require('./Profile/ProfileLayout');
var QualityLayout = require('./Quality/QualityLayout');
var IndexerLayout = require('./Indexers/IndexerLayout');
var IndexerCollection = require('./Indexers/IndexerCollection');
var IndexerSettingsModel = require('./Indexers/IndexerSettingsModel');
var DownloadClientLayout = require('./DownloadClient/DownloadClientLayout');
var DownloadClientSettingsModel = require('./DownloadClient/DownloadClientSettingsModel');
var NotificationCollectionView = require('./Notifications/NotificationCollectionView');
var NotificationCollection = require('./Notifications/NotificationCollection');
var MetadataLayout = require('./Metadata/MetadataLayout');
var GeneralView = require('./General/GeneralView');
var UiView = require('./UI/UiView');
var UiSettingsModel = require('./UI/UiSettingsModel');
var LoadingView = require('../Shared/LoadingView');
var Config = require('../Config');

module.exports = Marionette.Layout.extend({
    template                  : 'Settings/SettingsLayoutTemplate',
    regions                   : {
        mediaManagement : '#media-management',
        profiles        : '#profiles',
        quality         : '#quality',
        indexers        : '#indexers',
        downloadClient  : '#download-client',
        notifications   : '#notifications',
        metadata        : '#metadata',
        general         : '#general',
        uiRegion        : '#ui',
        loading         : '#loading-region'
    },
    ui                        : {
        mediaManagementTab : '.x-media-management-tab',
        profilesTab        : '.x-profiles-tab',
        qualityTab         : '.x-quality-tab',
        indexersTab        : '.x-indexers-tab',
        downloadClientTab  : '.x-download-client-tab',
        notificationsTab   : '.x-notifications-tab',
        metadataTab        : '.x-metadata-tab',
        generalTab         : '.x-general-tab',
        uiTab              : '.x-ui-tab',
        advancedSettings   : '.x-advanced-settings'
    },
    events                    : {
        "click .x-media-management-tab" : '_showMediaManagement',
        "click .x-profiles-tab"         : '_showProfiles',
        "click .x-quality-tab"          : '_showQuality',
        "click .x-indexers-tab"         : '_showIndexers',
        "click .x-download-client-tab"  : '_showDownloadClient',
        "click .x-notifications-tab"    : '_showNotifications',
        "click .x-metadata-tab"         : '_showMetadata',
        "click .x-general-tab"          : '_showGeneral',
        "click .x-ui-tab"               : '_showUi',
        "click .x-save-settings"        : '_save',
        "change .x-advanced-settings"   : '_toggleAdvancedSettings'
    },
    initialize                : function(options){
        if(options.action) {
            this.action = options.action.toLowerCase();
        }
        this.listenTo(vent, vent.Hotkeys.SaveSettings, this._save);
    },
    onRender                  : function(){
        this.loading.show(new LoadingView());
        var self = this;
        this.mediaManagementSettings = new MediaManagementSettingsModel();
        this.namingSettings = new NamingModel();
        this.indexerSettings = new IndexerSettingsModel();
        this.downloadClientSettings = new DownloadClientSettingsModel();
        this.notificationCollection = new NotificationCollection();
        this.generalSettings = new GeneralSettingsModel();
        this.uiSettings = new UiSettingsModel();
        Backbone.$.when(this.mediaManagementSettings.fetch(), this.namingSettings.fetch(), this.indexerSettings.fetch(), this.downloadClientSettings.fetch(), this.notificationCollection.fetch(), this.generalSettings.fetch(), this.uiSettings.fetch()).done(function(){
            if(!self.isClosed) {
                self.loading.$el.hide();
                self.mediaManagement.show(new MediaManagementLayout({
                    settings       : self.mediaManagementSettings,
                    namingSettings : self.namingSettings
                }));
                self.profiles.show(new ProfileLayout());
                self.quality.show(new QualityLayout());
                self.indexers.show(new IndexerLayout({model : self.indexerSettings}));
                self.downloadClient.show(new DownloadClientLayout({model : self.downloadClientSettings}));
                self.notifications.show(new NotificationCollectionView({collection : self.notificationCollection}));
                self.metadata.show(new MetadataLayout());
                self.general.show(new GeneralView({model : self.generalSettings}));
                self.uiRegion.show(new UiView({model : self.uiSettings}));
            }
        });
        this._setAdvancedSettingsState();
    },
    onShow                    : function(){
        switch (this.action) {
            case 'profiles':
                this._showProfiles();
                break;
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
            case 'ui':
                this._showUi();
                break;
            default:
                this._showMediaManagement();
        }
    },
    _showMediaManagement      : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.mediaManagementTab.tab('show');
        this._navigate('settings/mediamanagement');
    },
    _showProfiles             : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.profilesTab.tab('show');
        this._navigate('settings/profiles');
    },
    _showQuality              : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.qualityTab.tab('show');
        this._navigate('settings/quality');
    },
    _showIndexers             : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.indexersTab.tab('show');
        this._navigate('settings/indexers');
    },
    _showDownloadClient       : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.downloadClientTab.tab('show');
        this._navigate('settings/downloadclient');
    },
    _showNotifications        : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.notificationsTab.tab('show');
        this._navigate('settings/connect');
    },
    _showMetadata             : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.metadataTab.tab('show');
        this._navigate('settings/metadata');
    },
    _showGeneral              : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.generalTab.tab('show');
        this._navigate('settings/general');
    },
    _showUi                   : function(e){
        if(e) {
            e.preventDefault();
        }
        this.ui.uiTab.tab('show');
        this._navigate('settings/ui');
    },
    _navigate                 : function(route){
        Backbone.history.navigate(route, {
            trigger : false,
            replace : true
        });
    },
    _save                     : function(){
        vent.trigger(vent.Commands.SaveSettings);
    },
    _setAdvancedSettingsState : function(){
        var checked = Config.getValueBoolean(Config.Keys.AdvancedSettings);
        this.ui.advancedSettings.prop('checked', checked);
        if(checked) {
            $('body').addClass('show-advanced-settings');
        }
    },
    _toggleAdvancedSettings   : function(){
        var checked = this.ui.advancedSettings.prop('checked');
        Config.setValue(Config.Keys.AdvancedSettings, checked);
        if(checked) {
            $('body').addClass('show-advanced-settings');
        }
        else {
            $('body').removeClass('show-advanced-settings');
        }
    }
});