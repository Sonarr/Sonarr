'use strict';
define(['app', 'Shared/NotificationCollection', 'AddSeries/SearchResultCollection', 'AddSeries/SearchResultModel', 'Series/SeriesCollection'], function (app, notificationCollection) {

    NzbDrone.AddSeries.New.SearchItemView = Backbone.Marionette.ItemView.extend({

        template : "AddSeries/New/SearchResultTemplate",
        className: 'search-item',

        ui: {
            qualityProfile: '.x-quality-profile',
            rootFolder    : '.x-root-folder',
            addButton     : '.x-add'
        },

        events: {
            'click .x-add': 'add'
        },

        onRender: function () {
            this.listenTo(this.model, 'change', this.render);
        },

        add: function () {

            var seriesId = this.model.get('tvDbId');
            var title = this.model.get('title');
            var quality = this.ui.qualityProfile.val();
            var rootFolderId = this.ui.rootFolder.val();

            //Todo: This will create an invalid path on linux...
            var rootPath = this.model.get('rootFolders').get(rootFolderId).get('path');
            var path = rootPath + "\\" + title;

            var model = new NzbDrone.Series.SeriesModel({
                tvdbId          : seriesId,
                title           : title,
                qualityProfileId: quality,
                path            : path
            });

            var self = this;

            var seriesCollection = new NzbDrone.Series.SeriesCollection();
            seriesCollection.push(model);

            model.save(undefined, {
                success: function () {
                    var notificationModel = new NzbDrone.Shared.NotificationModel({
                        title  : 'Added',
                        message: title,
                        level  : 'success'
                    });

                    notificationCollection.push(notificationModel);
                    self.close();
                }
            });
        }
    });

    NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.CollectionView.extend({

        itemView  : NzbDrone.AddSeries.New.SearchItemView,
        className : 'accordion',
        initialize: function () {
            this.listenTo(this.collection, 'reset', this.render);
        }

    });
});
