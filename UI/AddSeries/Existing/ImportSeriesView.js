'use strict';
define([
    'app', 'AddSeries/RootFolders/RootFolderCollection', 'Quality/QualityProfileCollection', 'Shared/NotificationCollection', 'AddSeries/Existing/UnmappedFolderModel', 'Series/SeriesModel'], function (app, rootFolders, qualityProfileCollection, notificationCollection) {

    NzbDrone.AddSeries.Existing.FolderMatchResultView = Backbone.Marionette.ItemView.extend({
        template: 'AddSeries/SearchResultTemplate',

        events: {
            'click .x-add': 'addSeries'
        },

        addSeries: function () {

            var self = this;

            var quality = this.options.qualityProfile.val();
            var rootFolderId = this.options.rootFolder.id;
            var folder = this.options.folder.name;

            this.model.set('qualityProfileId', quality);
            this.model.set('rootFolderId', rootFolderId);
            this.model.set('folder', folder);

            var seriesCollection = new NzbDrone.Series.SeriesCollection();
            seriesCollection.add(this.model);

            this.model.save(undefined, {
                success: function () {
                    var notificationModel = new NzbDrone.Shared.NotificationModel({
                        tvDbId : self.model.get('tvDbId'),
                        title  : 'Added',
                        message: self.model.get('title'),
                        level  : 'success'
                    });

                    notificationCollection.push(notificationModel);
                    self.close();
                }
            });
        }
    });

    NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView = Backbone.Marionette.CompositeView.extend({

        template         : 'AddSeries/Existing/UnmappedFolderCompositeViewTemplate',
        itemViewContainer: '.x-folder-name-match-results',
        itemView         : NzbDrone.AddSeries.Existing.FolderMatchResultView,

        events: {
            'click .x-btn-search': 'search',
            'keydown .x-txt-search': 'keyDown'
        },

        ui: {
            searchButton: '.x-btn-search',
            searchText  : '.x-txt-search',
            profileList : '.x-lst-quality-profile'
        },

        initialize: function () {
            this.collection = new NzbDrone.Series.SeriesCollection();
            this.collection.bind('reset', this.collectionReset, this);
        },

        onRender: function () {
            this.collection.url = NzbDrone.Constants.ApiRoot + '/series/lookup';
            this.resultView = new NzbDrone.AddSeries.SearchResultView({ collection: this.collection });
        },

        search: function () {
            var icon = this.ui.searchButton.find('icon');

            this.collection.reset();
            icon.removeClass('icon-search').addClass('icon-spin icon-spinner disabled');

            this.collection.fetch({
                data   : { term: this.ui.searchText.val() },
                success: function (collection) {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                },
                fail   : function () {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                }
            });
        },

        keyDown: function (e) {
            //Check for enter being pressed
            var code = (e.keyCode ? e.keyCode : e.which);
            if(code === 13) {
                this.search();
            }
        },

        collectionReset: function () {
            _.each(this.collection.models, function (model){
                model.set('isExisting', true);
            });
        },

        itemViewOptions: function () {
            return {
                qualityProfile: this.ui.profileList,
                rootFolder    : this.model.get('rootFolder'),
                folder        : this.model.get('folder')
            };
        }
    });

    NzbDrone.AddSeries.Existing.RootFolderCompositeView = Backbone.Marionette.CompositeView.extend({

        template         : "AddSeries/Existing/RootFolderCompositeViewTemplate",
        itemViewContainer: ".x-existing-folder-container",
        itemView         : NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView,

        initialize: function () {

            if (!this.model) {
                throw "model is required.";
            }

            this.collection = new NzbDrone.AddSeries.Existing.UnmappedFolderCollection();
            this.refreshItems();
            this.listenTo(qualityProfileCollection, 'reset', this.refreshItems, this);
        },

        refreshItems: function () {
            this.collection.importItems(this.model);
        }
    });

    NzbDrone.AddSeries.Existing.ImportSeriesView = Backbone.Marionette.CollectionView.extend({

        itemView: NzbDrone.AddSeries.Existing.RootFolderCompositeView,

        initialize: function () {
            this.collection = rootFolders;
        }
    });
});
