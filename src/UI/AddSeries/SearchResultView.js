'use strict';
define(
    [
        'underscore',
        'vent',
        'AppLayout',
        'backbone',
        'marionette',
        'Profile/ProfileCollection',
        'AddSeries/RootFolders/RootFolderCollection',
        'AddSeries/RootFolders/RootFolderLayout',
        'Series/SeriesCollection',
        'Config',
        'Shared/Messenger',
        'Mixins/AsValidatedView',
        'jquery.dotdotdot'
    ], function (_,
                 vent,
                 AppLayout,
                 Backbone,
                 Marionette,
                 Profiles,
                 RootFolders,
                 RootFolderLayout,
                 SeriesCollection,
                 Config,
                 Messenger,
                 AsValidatedView) {

        var view = Marionette.ItemView.extend({

            template: 'AddSeries/SearchResultViewTemplate',

            ui: {
                profile        : '.x-profile',
                rootFolder     : '.x-root-folder',
                seasonFolder   : '.x-season-folder',
                seriesType     : '.x-series-type',
                startingSeason : '.x-starting-season',
                addButton      : '.x-add',
                overview       : '.x-overview'
            },

            events: {
                'click .x-add'            : '_addSeries',
                'change .x-profile'       : '_profileChanged',
                'change .x-root-folder'   : '_rootFolderChanged',
                'change .x-season-folder' : '_seasonFolderChanged',
                'change .x-series-type'   : '_seriesTypeChanged'
            },

            initialize: function () {

                if (!this.model) {
                    throw 'model is required';
                }

                this.templateHelpers = {};
                this._configureTemplateHelpers();

                this.listenTo(vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);
                this.listenTo(this.model, 'change', this.render);
                this.listenTo(RootFolders, 'all', this._rootFoldersUpdated);
            },

            onRender: function () {

                var defaultProfile = Config.getValue(Config.Keys.DefaultProfileId);
                var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);
                var useSeasonFolder = Config.getValueBoolean(Config.Keys.UseSeasonFolder, true);
                var defaultSeriesType = Config.getValue(Config.Keys.DefaultSeriesType, 'standard');

                if (Profiles.get(defaultProfile)) {
                    this.ui.profile.val(defaultProfile);
                }

                if (RootFolders.get(defaultRoot)) {
                    this.ui.rootFolder.val(defaultRoot);
                }

                this.ui.seasonFolder.prop('checked', useSeasonFolder);
                this.ui.seriesType.val(defaultSeriesType);

                var minSeasonNotZero = _.min(_.reject(this.model.get('seasons'), { seasonNumber: 0 }), 'seasonNumber');

                if (minSeasonNotZero) {
                    this.ui.startingSeason.val(minSeasonNotZero.seasonNumber);
                }

                //TODO: make this work via onRender, FM?
                //works with onShow, but stops working after the first render
                this.ui.overview.dotdotdot({
                    height: 120
                });
            },

            _configureTemplateHelpers: function () {
                var existingSeries = SeriesCollection.where({tvdbId: this.model.get('tvdbId')});

                if (existingSeries.length > 0) {
                    this.templateHelpers.existing = existingSeries[0].toJSON();
                }

                this.templateHelpers.profiles = Profiles.toJSON();

                if (!this.model.get('isExisting')) {
                    this.templateHelpers.rootFolders = RootFolders.toJSON();
                }
            },

            _onConfigUpdated: function (options) {
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
            },

            _profileChanged: function () {
                Config.setValue(Config.Keys.DefaultProfileId, this.ui.profile.val());
            },

            _seasonFolderChanged: function () {
                Config.setValue(Config.Keys.UseSeasonFolder, this.ui.seasonFolder.prop('checked'));
            },

            _rootFolderChanged: function () {
                var rootFolderValue = this.ui.rootFolder.val();
                if (rootFolderValue === 'addNew') {
                    var rootFolderLayout = new RootFolderLayout();
                    this.listenToOnce(rootFolderLayout, 'folderSelected', this._setRootFolder);
                    AppLayout.modalRegion.show(rootFolderLayout);
                }
                else {
                    Config.setValue(Config.Keys.DefaultRootFolderId, rootFolderValue);
                }
            },

            _seriesTypeChanged: function () {
                Config.setValue(Config.Keys.DefaultSeriesType, this.ui.seriesType.val());
            },

            _setRootFolder: function (options) {
                vent.trigger(vent.Commands.CloseModalCommand);
                this.ui.rootFolder.val(options.model.id);
                this._rootFolderChanged();
            },

            _addSeries: function () {
                var icon = this.ui.addButton.find('icon');
                icon.removeClass('icon-plus').addClass('icon-spin icon-spinner disabled');

                var profile = this.ui.profile.val();
                var rootFolderPath = this.ui.rootFolder.children(':selected').text();
                var startingSeason = this.ui.startingSeason.val();
                var seriesType = this.ui.seriesType.val();
                var seasonFolder = this.ui.seasonFolder.prop('checked');

                this.model.set({
                    profileId: profile,
                    rootFolderPath: rootFolderPath,
                    seasonFolder: seasonFolder,
                    seriesType: seriesType
                }, { silent: true });

                this.model.setSeasonPass(startingSeason);

                var self = this;
                var promise = this.model.save();

                promise.done(function () {
                    SeriesCollection.add(self.model);

                    self.close();
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');

                    Messenger.show({
                        message: 'Added: ' + self.model.get('title'),
                        actions : {
                            goToSeries: {
                                label: 'Go to Series Page',
                                action: function() {
                                    Backbone.history.navigate('/series/' + self.model.get('titleSlug'), { trigger: true });
                                }
                            }
                        },
                        hideAfter: 8,
                        hideOnNavigate: true
                    });

                    vent.trigger(vent.Events.SeriesAdded, { series: self.model });
                });

                promise.fail(function () {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                });
            },

            _rootFoldersUpdated: function () {
                this._configureTemplateHelpers();
                this.render();
            }
        });

        AsValidatedView.apply(view);

        return view;
    });
