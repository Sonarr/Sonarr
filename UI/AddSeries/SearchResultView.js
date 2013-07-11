'use strict';
define(
    [
        'app',
        'marionette',
        'Quality/QualityProfileCollection',
        'AddSeries/RootFolders/Collection',
        'Series/SeriesCollection',
        'Config',
        'Shared/Messenger'
    ], function (App, Marionette, QualityProfiles, RootFolders, SeriesCollection, Config, Messenger) {

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
                'change .x-quality-profile': '_qualityProfileChanged'
            },

            initialize: function () {

                if (!this.model) {
                    throw 'model is required';
                }

                this.listenTo(App.vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);
                this.listenTo(this.model, 'change', this.render);
            },

            onRender: function () {

                var defaultQuality = Config.GetValue(Config.Keys.DefaultQualityProfileId);

                if (QualityProfiles.get(defaultQuality)) {
                    this.ui.qualityProfile.val(defaultQuality);
                }
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
                    this.$('.x-quality-profile').val(options.value);
                }
            },

            _qualityProfileChanged: function () {
                Config.SetValue(Config.Keys.DefaultQualityProfileId, this.ui.qualityProfile.val());
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
