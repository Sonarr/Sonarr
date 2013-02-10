'use strict';
/*global NzbDrone, Backbone*/
/// <reference path="../../app.js" />
/// <reference path="../SearchResultModel.js" />
/// <reference path="../../Series/SeriesModel.js" />
/// <reference path="../SearchResultCollection.js" />

NzbDrone.AddSeries.SearchItemView = Backbone.Marionette.ItemView.extend({

    template: "AddSeries/New/SearchResultTemplate",
    className: 'search-item',

    ui: {
        qualityProfile: '.x-quality-profile',
        rootFolder: '.x-root-folder',
        addButton: '.x-add'
    },

    events: {
        'click .x-add': 'add'
    },

    onRender: function () {
        this.listenTo(this.model, 'change', this.render);
    },

    add: function () {

        var seriesId = this.model.get('id');
        var title = this.model.get('seriesName');
        var quality = this.ui.qualityProfile.val();
        var rootFolderId = this.ui.rootFolder.val();

        //Todo: This wiil create an invalid path on linux...
        var rootPath = this.model.get('rootFolders').get(rootFolderId).get('path');
        var path = rootPath + "\\" + title;

        var model = new NzbDrone.Series.SeriesModel({
            seriesId: seriesId,
            title: title,
            qualityProfileId: quality,
            path: path
        });

        var self = this;

        model.save(undefined, {
            success: function () {
                var notificationModel = new NzbDrone.Shared.NotificationModel({
                    title: 'Added',
                    message: title,
                    level: 'success'
                });

                NzbDrone.Shared.NotificationCollectionView.Instance.collection.add(notificationModel);
                self.close();
            }
        });
    }
});

NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.CollectionView.extend({

    itemView: NzbDrone.AddSeries.SearchItemView,

    className: 'accordion',

    initialize: function () {
        this.listenTo(this.collection, 'reset', this.render);
    }
});
