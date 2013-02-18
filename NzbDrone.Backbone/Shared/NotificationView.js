define(['app', 'Shared/NotificationCollection'], function (app, notificationCollection) {

    var notificationItemView = Backbone.Marionette.ItemView.extend({
        template: 'Shared/NotificationTemplate',

        events: {
            'click .x-close': 'kill'
        },

        kill: function () {

            var self = this;

            $.Deferred(function () {
                self.$el.slideUp('slow');
            }).done(function () {
                self.model.destroy();
            });
        }
    });

    var collectionView = Backbone.Marionette.CollectionView.extend({

        itemView: notificationItemView,

        initialize: function () {
            this.collection = notificationCollection;
        }
    });

    NzbDrone.addInitializer(function () {
        console.log('initializing notification view');
        NzbDrone.notificationRegion.show(new collectionView());
    });
    
});




