'use strict';
define([
    'app', 'AddSeries/RootFolders/RootFolderCollection',
    'Quality/QualityProfileCollection',
    'AddSeries/Existing/UnmappedFolderModel',
    'AddSeries/Collection',
    'Series/SeriesModel'], function (app, rootFolders, qualityProfileCollection) {

    NzbDrone.AddSeries.Existing.FolderMatchResultView = Backbone.Marionette.ItemView.extend({
        template: 'AddSeries/SearchResultTemplate',

        ui: {
            qualityProfile: '.x-quality-profile',
            addButton     : '.x-add'
        },

        events: {
            'click .x-add': 'addSeries'
        },

        addSeries: function () {
            var icon = this.ui.addButton.find('icon');
            icon.removeClass('icon-plus').addClass('icon-spin icon-spinner disabled');

            var self = this;

            var quality = this.ui.qualityProfile.val();
            var path = this.options.folder.path;

            this.model.set('qualityProfileId', quality);
            this.model.set('path', path);

            this.model.save(undefined, {
                success: function () {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    NzbDrone.Shared.Messenger.show({
                        message: 'Added: ' + self.model.get('title')
                    });

                    NzbDrone.vent.trigger(NzbDrone.Events.SeriesAdded, { existing: true, series: self.model });
                    self.trigger('seriesAdded');
                    self.close();
                },
                fail   : function () {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                }
            });
        }
    });

    NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView = Backbone.Marionette.CompositeView.extend({

        template         : 'AddSeries/Existing/UnmappedFolderCompositeViewTemplate',
        itemViewContainer: '.x-folder-name-match-results',
        itemView         : NzbDrone.AddSeries.Existing.FolderMatchResultView,

        events: {
            'click .x-btn-search'  : 'search',
            'keydown .x-txt-search': 'keyDown'
        },

        ui: {
            searchButton: '.x-btn-search',
            searchText  : '.x-txt-search',
            profileList : '.x-lst-quality-profile'
        },

        initialize: function () {
            this.collection = new NzbDrone.AddSeries.Collection();
            this.collection.bind('reset', this.collectionReset, this);

            this.on("itemview:seriesAdded", function () {
                this.close();
            });
        },

        onRender: function () {
            this.resultView = new NzbDrone.AddSeries.SearchResultView({ collection: this.collection });
        },

        search: function () {
            var icon = this.ui.searchButton.find('icon');

            var deferred = $.Deferred();

            this.collection.reset();
            icon.removeClass('icon-search').addClass('icon-spin icon-spinner disabled');

            this.collection.fetch({
                data   : { term: this.ui.searchText.val() },
                success: function (collection) {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    deferred.resolve();
                },
                fail   : function () {
                    icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    deferred.reject();
                }
            });

            return deferred.promise();
        },

        keyDown: function (e) {
            //Check for enter being pressed
            var code = (e.keyCode ? e.keyCode :e.which);
            if (code === 13) {
                this.search();
            }
        },

        collectionReset: function () {
            _.each(this.collection.models, function (model) {
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
        },

        showCollection: function () {
            this.showAndSearch(0);
        },

        showAndSearch: function (index) {

            var model = this.collection.at(index);
            if (model) {
                var that = this;
                var currentIndex = index;
                this.addItemView(model, this.getItemView(), index);
                console.log('start');
                $.when(this.children.findByModel(model).search())
                    .then(function () {
                        console.log('done');
                        that.showAndSearch(currentIndex + 1);
                    });
            }
        }

    });
});
