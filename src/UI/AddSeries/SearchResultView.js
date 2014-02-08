'use strict';
define(
    [
        'vent',
        'AppLayout',
        'underscore',
        'marionette',
        'Quality/QualityProfileCollection',
        'AddSeries/RootFolders/RootFolderCollection',
        'AddSeries/RootFolders/RootFolderLayout',
        'Series/SeriesCollection',
        'Config',
        'Shared/Messenger',
        'Mixins/AsValidatedView',
        'jquery.dotdotdot'
    ], function (vent, AppLayout, _, Marionette, QualityProfiles, RootFolders, RootFolderLayout, SeriesCollection, Config, Messenger, AsValidatedView) {

        var view = Marionette.ItemView.extend({

            template: 'AddSeries/SearchResultViewTemplate',

            ui: {
                qualityProfile: '.x-quality-profile',
                rootFolder    : '.x-root-folder',
                seasonFolder  : '.x-season-folder',
                addButton     : '.x-add',
                overview      : '.x-overview',
                startingSeason: '.x-starting-season'
            },

            events: {
                'click .x-add'             : '_addSeries',
                'change .x-quality-profile': '_qualityProfileChanged',
                'change .x-root-folder'    : '_rootFolderChanged',
                'change .x-season-folder'  : '_seasonFolderChanged'
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

                var defaultQuality = Config.getValue(Config.Keys.DefaultQualityProfileId);
                var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);
                var useSeasonFolder = Config.getValueBoolean(Config.Keys.UseSeasonFolder, true);

                if (QualityProfiles.get(defaultQuality)) {
                    this.ui.qualityProfile.val(defaultQuality);
                }

                if (RootFolders.get(defaultRoot)) {
                    this.ui.rootFolder.val(defaultRoot);
                }

                this.ui.seasonFolder.prop('checked', useSeasonFolder);

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

                this.templateHelpers.qualityProfiles = QualityProfiles.toJSON();

                if (!this.model.get('isExisting')) {
                    this.templateHelpers.rootFolders = RootFolders.toJSON();
                }
            },

            _onConfigUpdated: function (options) {
                if (options.key === Config.Keys.DefaultQualityProfileId) {
                    this.ui.qualityProfile.val(options.value);
                }

                else if (options.key === Config.Keys.DefaultRootFolderId) {
                    this.ui.rootFolder.val(options.value);
                }

                else if (options.key === Config.Keys.UseSeasonFolder) {
                    this.ui.seasonFolder.prop('checked', options.value);
                }
            },

            _qualityProfileChanged: function () {
                Config.setValue(Config.Keys.DefaultQualityProfileId, this.ui.qualityProfile.val());
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

            _setRootFolder: function (options) {
                vent.trigger(vent.Commands.CloseModalCommand);
                this.ui.rootFolder.val(options.model.id);
                this._rootFolderChanged();
            },

            _addSeries: function () {
                var icon = this.ui.addButton.find('icon');
                icon.removeClass('icon-plus').addClass('icon-spin icon-spinner disabled');

                var quality = this.ui.qualityProfile.val();
                var rootFolderPath = this.ui.rootFolder.children(':selected').text();
                var startingSeason = this.ui.startingSeason.val();
                var seasonFolder = this.ui.seasonFolder.prop('checked');

                this.model.set('qualityProfileId', quality);
                this.model.set('rootFolderPath', rootFolderPath);
                this.model.setSeasonPass(startingSeason);
                this.model.set('seasonFolder', seasonFolder);

                var self = this;
                var promise = this.model.save();

                promise.done(function () {
                    SeriesCollection.add(self.model);

                    self.close();
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');

                    Messenger.show({
                        message: 'Added: ' + self.model.get('title')
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
