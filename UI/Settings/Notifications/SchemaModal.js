'use strict';
define([
    'app',
    'Settings/Notifications/Collection',
    'Settings/Notifications/AddView'
], function (App, NotificationCollection, AddSelectionNotificationView) {
    return ({

        open: function (collection) {
            var schemaCollection = new NotificationCollection();
            schemaCollection.url = '/api/notification/schema';
            schemaCollection.fetch();
            schemaCollection.url = '/api/notification';

            var view = new AddSelectionNotificationView({ collection: schemaCollection, notificationCollection: collection});
            App.modalRegion.show(view);
        }
    });
});
