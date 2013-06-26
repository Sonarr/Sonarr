'use strict';
define([
    'app',
    'marionette',
    'Settings/Notifications/Collection',
    'Settings/Notifications/ItemView',
    'Settings/Notifications/AddView'
], function (App, Marionette, NotificationCollection, NotificationItemView, AddSelectionNotificationView) {
    return Marionette.CompositeView.extend({
        itemView         : NotificationItemView,
        itemViewContainer: '.notifications',
        template         : 'Settings/Notifications/CollectionTemplate',

        events: {
            'click .x-add': '_openSchemaModal'
        },

        _openSchemaModal: function () {
            var schemaCollection = new NotificationCollection();
            schemaCollection.url = '/api/notification/schema';
            schemaCollection.fetch();
            schemaCollection.url = '/api/notification';

            var view = new AddSelectionNotificationView({ collection: schemaCollection, notificationCollection: this.collection});
            App.modalRegion.show(view);
        }
    });
});
