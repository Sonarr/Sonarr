'use strict';
define(['app', 'Series/SeriesCollection', 'AddSeries/RootFolders/RootFolderTemplateHelper'], function () {

    NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.ItemView.extend({

        template: "AddSeries/SearchResultTemplate",

        ui: {
            qualityProfile: '.x-quality-profile',
            rootFolder    : '.x-root-folder',
            addButton     : '.x-add',
            overview      : '.x-overview'
        },

        events: {
            'click .x-add': 'addSeries'
        },

        initialize: function () {

            if (!this.model) {
                throw 'model is required';
            }

            this.model.set('isExisting', this.options.isExisting);
            this.model.set('path', this.options.folder);
        },

        onRender: function () {
            this.listenTo(this.model, 'change', this.render);
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
