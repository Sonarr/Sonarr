'use strict';
define([
    'AppLayout',
    'Settings/Notifications/Collection',
    'Settings/Notifications/AddView'
], function (AppLayout, NotificationCollection, AddSelectionNotificationView) {
    return ({

        open: function (collection) {
            var schemaCollection = new NotificationCollection();
            schemaCollection.url = '/api/notification/schema';
            schemaCollection.fetch();
            schemaCollection.url = '/api/notification';

            var view = new AddSelectionNotificationView({ collection: schemaCollection, notificationCollection: collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
