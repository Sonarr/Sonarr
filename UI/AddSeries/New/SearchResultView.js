'use strict';
define(['app', 'Shared/NotificationCollection', 'AddSeries/SearchResultCollection', 'AddSeries/SearchResultModel', 'Series/SeriesCollection'], function (app, notificationCollection) {

    NzbDrone.AddSeries.New.SearchItemView = Backbone.Marionette.ItemView.extend({

        template : "AddSeries/SearchResultTemplate",

        ui: {
            qualityProfile: '.x-quality-profile',
            rootFolder    : '.x-root-folder',
            addButton     : '.x-add'
        },

        events: {
            'click .x-add': 'addSeries'
        },

        onRender: function () {
            this.listenTo(this.model, 'change', this.render);
        },

        addSeries: function () {

            var quality = this.ui.qualityProfile.val();

            //Todo: This will create an invalid path on linux...
            var rootPath = this.ui.rootFolder.find(":selected").text();
            var path = rootPath + "\\" + this.model.get('title');

            this.model.set('qualityProfileId', quality);
            this.model.set('path', path);

            var self = this;

            this.model.save(undefined, {
                success: function () {
                    var notificationModel = new NzbDrone.Shared.NotificationModel({
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

    NzbDrone.AddSeries.SearchResultView = Backbone.Marionette.CollectionView.extend({

        itemView  : NzbDrone.AddSeries.New.SearchItemView,
        initialize: function () {
            this.listenTo(this.collection, 'reset', this.render);
        }

    });
});
