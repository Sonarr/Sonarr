'use strict';
define(
    [
        'app',
        'marionette',

        'Quality/QualityProfileCollection',
        'AddSeries/RootFolders/Collection',
        'AddSeries/RootFolders/Layout',
        'Series/SeriesCollection',
        'Config',
        'Shared/Messenger',
        'jquery.dotdotdot'
    ], function (App, Marionette, QualityProfiles, RootFolders, RootFolderLayout, SeriesCollection, Config, Messenger) {

        return Marionette.ItemView.extend({

            template: 'AddSeries/SearchResultTemplate',

            ui: {
                qualityProfile: '.x-quality-profile',
                rootFolder    : '.x-root-folder',
                addButton     : '.x-add',
                overview      : '.x-overview'
            },

            events: {
                'click .x-add'             : '_addSeries',
                'change .x-quality-profile': '_qualityProfileChanged',
                'change .x-root-folder'    : '_rootFolderChanged'
            },

            initialize: function () {

                if (!this.model) {
                    throw 'model is required';
                }

                this.listenTo(App.vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);
                this.listenTo(this.model, 'change', this.render);
                this.listenTo(RootFolders, 'change', this.render);

                this.rootFolderLayout = new RootFolderLayout();
                this.listenTo(this.rootFolderLayout, 'folderSelected', this._setRootFolder);
            },

            onRender: function () {

                var defaultQuality = Config.getValue(Config.Keys.DefaultQualityProfileId);
                var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);

                if (QualityProfiles.get(defaultQuality)) {
                    this.ui.qualityProfile.val(defaultQuality);
                }

                if (RootFolders.get(defaultRoot)) {
                    this.ui.rootFolder.val(defaultRoot);
                }

                //TODO: make this work via onRender, FM?
                //works with onShow, but stops working after the first render
                this.ui.overview.dotdotdot({
                    height: 120
                });
            },

            serializeData: function () {
                var data = this.model.toJSON();

                var existingSeries = SeriesCollection.where({tvdbId: this.model.get('tvdbId')});

                if (existingSeries.length > 0) {
                    data.existing = existingSeries[0].toJSON();
                }

                data.qualityProfiles = QualityProfiles.toJSON();

                if (!data.isExisting) {
                    data.rootFolders = RootFolders.toJSON();
                }

                return data;
            },

            _onConfigUpdated: function (options) {
                if (options.key === Config.Keys.DefaultQualityProfileId) {
                    this.ui.qualityProfile.val(options.value);
                }
                else if (options.key === Config.Keys.DefaultRootFolderId) {
                    this.ui.rootFolder.val(options.value);
                }
            },

            _qualityProfileChanged: function () {
                Config.setValue(Config.Keys.DefaultQualityProfileId, this.ui.qualityProfile.val());
            },

            _rootFolderChanged: function () {
                var rootFolderValue = this.ui.rootFolder.val();
                if (rootFolderValue === 'addNew') {
                    App.modalRegion.show(this.rootFolderLayout);
                }
                else {
                    Config.setValue(Config.Keys.DefaultRootFolderId, rootFolderValue);
                }
            },

            _setRootFolder: function (options) {
                App.vent.trigger(App.Commands.CloseModalCommand);
                this.ui.rootFolder.val(options.model.id);
            },

            _addSeries: function () {
                var icon = this.ui.addButton.find('icon');
                icon.removeClass('icon-plus').addClass('icon-spin icon-spinner disabled');

                var quality = this.ui.qualityProfile.val();
                var rootFolderPath = this.ui.rootFolder.children(':selected').text();

                this.model.set('qualityProfileId', quality);
                this.model.set('rootFolderPath', rootFolderPath);

                var self = this;

                SeriesCollection.add(this.model);


                this.model.save().done(function () {
                    self.close();
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    Messenger.show({
                        message: 'Added: ' + self.model.get('title')
                    });

                    App.vent.trigger(App.Events.SeriesAdded, { series: self.model });
                }).fail(function () {
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    });
            }
        });
    });
