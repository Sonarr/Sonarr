var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../AppLayout');
var Backbone = require('backbone');
var Marionette = require('marionette');
var Profiles = require('../Profile/ProfileCollection');
var RootFolders = require('./RootFolders/RootFolderCollection');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var SeriesCollection = require('../Series/SeriesCollection');
var Config = require('../Config');
var Messenger = require('../Shared/Messenger');
var AsValidatedView = require('../Mixins/AsValidatedView');

require('jquery.dotdotdot');

var view = Marionette.ItemView.extend({

    template : 'AddSeries/SearchResultViewTemplate',

    ui : {
        profile         : '.x-profile',
        rootFolder      : '.x-root-folder',
        seasonFolder    : '.x-season-folder',
        seriesType      : '.x-series-type',
        monitor         : '.x-monitor',
        monitorTooltip  : '.x-monitor-tooltip',
        addButton       : '.x-add',
        addSearchButton : '.x-add-search',
        overview        : '.x-overview'
    },

    events : {
        'click .x-add'            : '_addWithoutSearch',
        'click .x-add-search'     : '_addAndSearch',
        'change .x-profile'       : '_profileChanged',
        'change .x-root-folder'   : '_rootFolderChanged',
        'change .x-season-folder' : '_seasonFolderChanged',
        'change .x-series-type'   : '_seriesTypeChanged',
        'change .x-monitor'       : '_monitorChanged'
    },

    initialize : function() {

        if (!this.model) {
            throw 'model is required';
        }

        this.templateHelpers = {};
        this._configureTemplateHelpers();

        this.listenTo(vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);
        this.listenTo(this.model, 'change', this.render);
        this.listenTo(RootFolders, 'all', this._rootFoldersUpdated);
    },

    onRender : function() {

        var defaultProfile = Config.getValue(Config.Keys.DefaultProfileId);
        var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);
        var useSeasonFolder = Config.getValueBoolean(Config.Keys.UseSeasonFolder, true);
        var defaultSeriesType = Config.getValue(Config.Keys.DefaultSeriesType, 'standard');
        var defaultMonitorEpisodes = Config.getValue(Config.Keys.MonitorEpisodes, 'missing');

        if (Profiles.get(defaultProfile)) {
            this.ui.profile.val(defaultProfile);
        }

        if (RootFolders.get(defaultRoot)) {
            this.ui.rootFolder.val(defaultRoot);
        }

        this.ui.seasonFolder.prop('checked', useSeasonFolder);
        this.ui.seriesType.val(defaultSeriesType);
        this.ui.monitor.val(defaultMonitorEpisodes);

        //TODO: make this work via onRender, FM?
        //works with onShow, but stops working after the first render
        this.ui.overview.dotdotdot({
            height : 120
        });

        this.templateFunction = Marionette.TemplateCache.get('AddSeries/MonitoringTooltipTemplate');
        var content = this.templateFunction();

        this.ui.monitorTooltip.popover({
            content   : content,
            html      : true,
            trigger   : 'hover',
            title     : 'Episode Monitoring Options',
            placement : 'right',
            container : this.$el
        });
    },

    _configureTemplateHelpers : function() {
        var existingSeries = SeriesCollection.where({ tvdbId : this.model.get('tvdbId') });

        if (existingSeries.length > 0) {
            this.templateHelpers.existing = existingSeries[0].toJSON();
        }

        this.templateHelpers.profiles = Profiles.toJSON();

        if (!this.model.get('isExisting')) {
            this.templateHelpers.rootFolders = RootFolders.toJSON();
        }
    },

    _onConfigUpdated : function(options) {
        if (options.key === Config.Keys.DefaultProfileId) {
            this.ui.profile.val(options.value);
        }

        else if (options.key === Config.Keys.DefaultRootFolderId) {
            this.ui.rootFolder.val(options.value);
        }

        else if (options.key === Config.Keys.UseSeasonFolder) {
            this.ui.seasonFolder.prop('checked', options.value);
        }

        else if (options.key === Config.Keys.DefaultSeriesType) {
            this.ui.seriesType.val(options.value);
        }

        else if (options.key === Config.Keys.MonitorEpisodes) {
            this.ui.monitor.val(options.value);
        }
    },

    _profileChanged : function() {
        Config.setValue(Config.Keys.DefaultProfileId, this.ui.profile.val());
    },

    _seasonFolderChanged : function() {
        Config.setValue(Config.Keys.UseSeasonFolder, this.ui.seasonFolder.prop('checked'));
    },

    _rootFolderChanged : function() {
        var rootFolderValue = this.ui.rootFolder.val();
        if (rootFolderValue === 'addNew') {
            var rootFolderLayout = new RootFolderLayout();
            this.listenToOnce(rootFolderLayout, 'folderSelected', this._setRootFolder);
            AppLayout.modalRegion.show(rootFolderLayout);
        } else {
            Config.setValue(Config.Keys.DefaultRootFolderId, rootFolderValue);
        }
    },

    _seriesTypeChanged : function() {
        Config.setValue(Config.Keys.DefaultSeriesType, this.ui.seriesType.val());
    },

    _monitorChanged : function() {
        Config.setValue(Config.Keys.MonitorEpisodes, this.ui.monitor.val());
    },

    _setRootFolder : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);
        this.ui.rootFolder.val(options.model.id);
        this._rootFolderChanged();
    },

    _addWithoutSearch : function() {
        this._addSeries(false);
    },

    _addAndSearch : function() {
        this._addSeries(true);
    },

    _addSeries : function(searchForMissingEpisodes) {
        var addButton = this.ui.addButton;
        var addSearchButton = this.ui.addSearchButton;

        addButton.addClass('disabled');
        addSearchButton.addClass('disabled');

        var profile = this.ui.profile.val();
        var rootFolderPath = this.ui.rootFolder.children(':selected').text();
        var seriesType = this.ui.seriesType.val();
        var seasonFolder = this.ui.seasonFolder.prop('checked');

        var options = this._getAddSeriesOptions();
        options.searchForMissingEpisodes = searchForMissingEpisodes;

        this.model.set({
            profileId      : profile,
            rootFolderPath : rootFolderPath,
            seasonFolder   : seasonFolder,
            seriesType     : seriesType,
            addOptions     : options,
            monitored      : true
        }, { silent : true });

        var self = this;
        var promise = this.model.save();

        if (searchForMissingEpisodes) {
            this.ui.addSearchButton.spinForPromise(promise);
        }

        else {
            this.ui.addButton.spinForPromise(promise);
        }

        promise.always(function() {
            addButton.removeClass('disabled');
            addSearchButton.removeClass('disabled');
        });

        promise.done(function() {
            SeriesCollection.add(self.model);

            self.close();

            Messenger.show({
                message        : 'Added: ' + self.model.get('title'),
                actions        : {
                    goToSeries : {
                        label  : 'Go to Series',
                        action : function() {
                            Backbone.history.navigate('/series/' + self.model.get('titleSlug'), { trigger : true });
                        }
                    }
                },
                hideAfter      : 8,
                hideOnNavigate : true
            });

            vent.trigger(vent.Events.SeriesAdded, { series : self.model });
        });
    },

    _rootFoldersUpdated : function() {
        this._configureTemplateHelpers();
        this.render();
    },

    _getAddSeriesOptions : function() {
        var monitor = this.ui.monitor.val();
        var lastSeason = _.max(this.model.get('seasons'), 'seasonNumber');
        var firstSeason = _.min(_.reject(this.model.get('seasons'), { seasonNumber : 0 }), 'seasonNumber');

        this.model.setSeasonPass(firstSeason.seasonNumber);

        var options = {
            ignoreEpisodesWithFiles    : false,
            ignoreEpisodesWithoutFiles : false
        };

        if (monitor === 'all') {
            return options;
        }

        else if (monitor === 'future') {
            options.ignoreEpisodesWithFiles = true;
            options.ignoreEpisodesWithoutFiles = true;
        }

        else if (monitor === 'latest') {
            this.model.setSeasonPass(lastSeason.seasonNumber);
        }

        else if (monitor === 'first') {
            this.model.setSeasonPass(lastSeason.seasonNumber + 1);
            this.model.setSeasonMonitored(firstSeason.seasonNumber);
        }

        else if (monitor === 'missing') {
            options.ignoreEpisodesWithFiles = true;
        }

        else if (monitor === 'existing') {
            options.ignoreEpisodesWithoutFiles = true;
        }

        else if (monitor === 'none') {
            this.model.setSeasonPass(lastSeason.seasonNumber + 1);
        }

        return options;
    }
});

AsValidatedView.apply(view);

module.exports = view;
