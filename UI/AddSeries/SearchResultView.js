'use strict';
define(
    [
        'app',
        'marionette',
        'Config',
        'Series/SeriesCollection',
        'Shared/Messenger',
        'Quality/QualityProfileCollection'
    ], function (App, Marionette, Config, SeriesCollection, Messenger, QualityProfiles) {

        return Marionette.ItemView.extend({

            template: "AddSeries/SearchResultTemplate",

            ui: {
                qualityProfile: '.x-quality-profile',
                rootFolder    : '.x-root-folder',
                addButton     : '.x-add',
                overview      : '.x-overview'
            },

            events: {
                'click .x-add'             : 'addSeries',
                'change .x-quality-profile': '_qualityProfileChanged'
            },

            initialize: function () {

                if (!this.model) {
                    throw 'model is required';
                }

                this.model.set('isExisting', this.options.isExisting);
                this.model.set('path', this.options.folder);

                App.vent.on(Config.Events.ConfigUpdatedEvent, this._onConfigUpdated, this);
            },


            _onConfigUpdated: function (options) {

                if (options.key === Config.Keys.DefaultQualityProfileId) {
                    this.$('.x-quality-profile').val(options.value);
                }
            },

            _qualityProfileChanged: function () {
                Config.SetValue(Config.Keys.DefaultQualityProfileId, this.ui.qualityProfile.val());
            },

            onRender: function () {
                this.listenTo(this.model, 'change', this.render);

                var defaultQuality = Config.GetValue(Config.Keys.DefaultQualityProfileId);

                if (QualityProfiles.get(defaultQuality)) {
                    this.ui.qualityProfile.val(defaultQuality);
                }
            },


            addSeries: function () {
                var icon = this.ui.addButton.find('icon');
                icon.removeClass('icon-plus').addClass('icon-spin icon-spinner disabled');

                var quality = this.ui.qualityProfile.val();
                var rootFolderPath = this.ui.rootFolder.children(':selected').text();

                this.model.set('qualityProfileId', quality);
                this.model.set('rootFolderPath', rootFolderPath);

                var self = this;

                this.model.save(undefined, {
                    url    : SeriesCollection.prototype.url,
                    success: function () {
                        self.close();
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                        Messenger.show({
                            message: 'Added: ' + self.model.get('title')
                        });

                        App.vent.trigger(App.Events.SeriesAdded, { series: self.model });
                        self.model.collection.remove(self.model);
                    },
                    fail   : function () {
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    }
                });
            }
        });


    });
