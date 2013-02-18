'use strict';

define([
    'app', 'AddSeries/RootFolders/RootFolderCollection', 'Quality/QualityProfileCollection', 'Shared/NotificationCollection', 'AddSeries/Existing/UnmappedFolderModel', 'AddSeries/SearchResultCollection', 'Series/SeriesModel'], function (app, rootFolders, qualityProfileCollection, notificationCollection) {


        NzbDrone.AddSeries.Existing.FolderMatchResultView = Backbone.Marionette.ItemView.extend({
            template:'AddSeries/Existing/FolderMatchResultViewTemplatate',

            events:{
                'click .x-btn-add':'addSeries'
            },

            addSeries:function () {

                var self = this;

                var seriesId = this.model.get('id');
                var title = this.model.get('seriesName');
                var quality = this.options.qualityProfile.val();
                var path = this.options.folder.path;

                var model = new NzbDrone.Series.SeriesModel({
                    seriesId:seriesId,
                    title:title,
                    qualityProfileId:quality,
                    path:path
                });


                var seriesCollection = new NzbDrone.Series.SeriesCollection();
                seriesCollection.add(model);

                model.save(undefined, {
                    success:function () {
                        var notificationModel = new NzbDrone.Shared.NotificationModel({
                            title:'Added',
                            message:title,
                            level:'success'
                        });

                        notificationCollection.push(notificationModel);
                        self.close();
                    }
                });
            }

        });

        NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView = Backbone.Marionette.CompositeView.extend({

            template:'AddSeries/Existing/UnmappedFolderCompositeViewTemplatate',
            itemViewContainer:'.x-folder-name-match-results',
            itemView:NzbDrone.AddSeries.Existing.FolderMatchResultView,

            events:{
                'click .x-btn-search':'search'
            },

            ui:{
                searchButton:'.x-btn-search',
                searchText:'.x-txt-search',
                profileList:'.x-lst-quality-profile'
            },

            initialize:function () {
                this.collection = new NzbDrone.AddSeries.SearchResultCollection();
            },

            search:function () {

                var icon = this.ui.searchButton.find('icon');

                icon.removeClass('icon-search').addClass('icon-spin icon-spinner disabled');

                this.collection.fetch({
                    data:{ term:this.ui.searchText.val() },
                    success:function () {
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');

                    },
                    fail:function () {
                        icon.removeClass('icon-spin icon-spinner disabled').addClass('icon-search');
                    }
                });
            },

            itemViewOptions:function () {
                return {
                    qualityProfile:this.ui.profileList,
                    rootFolder: this.model.get('rootFolder'),
                    folder: this.model.get('folder')
                };
            }



        });

        NzbDrone.AddSeries.Existing.RootFolderCompositeView = Backbone.Marionette.CompositeView.extend({

            template:"AddSeries/Existing/RootFolderCompositeViewTemplate",
            itemViewContainer:".x-existing-folder-container",
            itemView:NzbDrone.AddSeries.Existing.UnmappedFolderCompositeView,

            initialize:function () {

                if (!this.model) {
                    throw "model is required.";
                }

                this.collection = new NzbDrone.AddSeries.Existing.UnmappedFolderCollection();
                this.refreshItems();
                this.listenTo(qualityProfileCollection, 'reset', this.refreshItems, this);
            },

            refreshItems:function () {
                this.collection.importItems(this.model);
            }



        });

        NzbDrone.AddSeries.Existing.ImportSeriesView = Backbone.Marionette.CollectionView.extend({

            itemView:NzbDrone.AddSeries.Existing.RootFolderCompositeView,

            initialize:function () {
                this.collection = rootFolders;
            }

        });
    });
