'use strict';
define(['app', 'Settings/Notifications/ItemView', 'Settings/Notifications/AddView'], function () {
    NzbDrone.Settings.Notifications.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Notifications.ItemView,
        itemViewContainer       : 'tbody',
        template                : 'Settings/Notifications/CollectionTemplate',

        events: {
            'click .x-add': 'openSchemaModal'
        },

        openSchemaModal: function () {
            var schemaCollection = new NzbDrone.Settings.Notifications.Collection();
            schemaCollection.url = '/api/notification/schema';
            schemaCollection.fetch();
            schemaCollection.url = '/api/notification';

            var view = new NzbDrone.Settings.Notifications.AddView({ collection: schemaCollection, notificationCollection: this.collection});
            NzbDrone.modalRegion.show(view);
        }
    });
});
