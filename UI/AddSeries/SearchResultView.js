'use strict';
define(['app',
    'Quality/QualityProfileCollection',
    'Config',
    'Series/SeriesCollection',
    'AddSeries/RootFolders/RootFolderTemplateHelper',
    'Quality/QualityProfileTemplateHelper'], function (app, qualityProfiles) {

    NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.ItemView.extend({

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

            NzbDrone.vent.on(NzbDrone.Config.Events.ConfigUpdatedEvent, this._onConfigUpdated, this);
        },


        _onConfigUpdated: function (options) {

            if (options.key === NzbDrone.Config.Keys.DefaultQualityProfileId) {
                this.$('.x-quality-profile').val(options.value);
            }
        },

        _qualityProfileChanged: function () {
            NzbDrone.Config.SetValue(NzbDrone.Config.Keys.DefaultQualityProfileId, this.ui.qualityProfile.val());
        },

        onRender: function () {
            this.listenTo(this.model, 'change', this.render);

            var defaultQuality = NzbDrone.Config.GetValue(NzbDrone.Config.Keys.DefaultQualityProfileId);

            if (qualityProfiles.get(defaultQuality)) {
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
                url    : NzbDrone.Series.SeriesCollection.prototype.url,
                success: function () {
                    self.close();
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    NzbDrone.Shared.Messenger.show({
                        message: 'Added: ' + self.model.get('title')
                    });

                    NzbDrone.vent.trigger(NzbDrone.Events.SeriesAdded, { series: self.model });
                    self.model.collection.remove(self.model);
                },
                fail   : function () {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                }
            });
        }
    });


    NzbDrone.AddSeries.SearchResultCollectionView = Backbone.Marionette.CollectionView.extend({

        itemView  : NzbDrone.AddSeries.SearchResultView,
        initialize: function () {
            this.listenTo(this.collection, 'reset', this.render);
        }

    });

});
