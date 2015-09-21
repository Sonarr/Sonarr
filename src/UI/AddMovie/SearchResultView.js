var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../AppLayout');
var Backbone = require('backbone');
var Marionette = require('marionette');
var Profiles = require('../Profile/ProfileCollection');
var RootFolders = require('../Shared/RootFolders/RootFolderCollection');
var RootFolderLayout = require('../Shared/RootFolders/RootFolderLayout');
var MovieCollection = require('../Movie/MovieCollection');
var Config = require('../Config');
var Messenger = require('../Shared/Messenger');
var AsValidatedView = require('../Mixins/AsValidatedView');

require('jquery.dotdotdot');

var view = Marionette.ItemView.extend({

    template : 'AddMovie/SearchResultViewTemplate',

    ui : {
        profile         : '.x-profile',
        rootFolder      : '.x-root-folder',
        addButton       : '.x-add',
        addSearchButton : '.x-add-search',
        overview        : '.x-overview'
    },

    events : {
        'click .x-add'            : '_addWithoutSearch',
        'click .x-add-search'     : '_addAndSearch',
        'change .x-profile'       : '_profileChanged',
        'change .x-root-folder'   : '_rootFolderChanged',
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
        var defaultMonitorEpisodes = Config.getValue(Config.Keys.MonitorEpisodes, 'missing');

        if (Profiles.get(defaultProfile)) {
            this.ui.profile.val(defaultProfile);
        }

        if (RootFolders.get(defaultRoot)) {
            this.ui.rootFolder.val(defaultRoot);
        }

        //TODO: make this work via onRender, FM?
        //works with onShow, but stops working after the first render
        this.ui.overview.dotdotdot({
            height : 120
        });

        //var content = this.templateFunction();
    },

    _configureTemplateHelpers : function() {
        var existingMovies = MovieCollection.where({ tmdbId : this.model.get('tmdbId') });

        if (existingMovies.length > 0) {
            this.templateHelpers.existing = existingMovies[0].toJSON();
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
    },

    _profileChanged : function() {
        Config.setValue(Config.Keys.DefaultProfileId, this.ui.profile.val());
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

    _setRootFolder : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);
        this.ui.rootFolder.val(options.model.id);
        this._rootFolderChanged();
    },

    _addWithoutSearch : function() {
        this._addMovie(false);
    },

    _addAndSearch : function() {
        this._addMovie(true);
    },

    _addMovie : function(searchForMissingFile) {
        var addButton = this.ui.addButton;
        var addSearchButton = this.ui.addSearchButton;

        addButton.addClass('disabled');
        addSearchButton.addClass('disabled');

        var profile = this.ui.profile.val();
        var rootFolderPath = this.ui.rootFolder.children(':selected').text();

        this.model.set({
            profileId      : profile,
            rootFolderPath : rootFolderPath,
            addOptions     : searchForMissingFile
        }, { silent : true });

        var self = this;
        var promise = this.model.save();

        if (searchForMissingFile) {
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
            MovieCollection.add(self.model);

            self.close();

            Messenger.show({
                message        : 'Added: ' + self.model.get('title'),
                actions        : {
                    goToSeries : {
                        label  : 'Go to Movies',
                        action : function() {
                            Backbone.history.navigate('/movie/' + self.model.get('cleanTitle'), { trigger : true });
                        }
                    }
                },
                hideAfter      : 8,
                hideOnNavigate : true
            });

            vent.trigger(vent.Events.MovieAdded, { movie : self.model });
        });
    },

    _rootFoldersUpdated : function() {
        this._configureTemplateHelpers();
        this.render();
    }
});

AsValidatedView.apply(view);

module.exports = view;