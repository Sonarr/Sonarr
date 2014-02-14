'use strict';
define([
    'AppLayout',
    'Settings/Notifications/Collection',
    'Settings/Notifications/AddView'
], function (AppLayout, NotificationCollection, AddSelectionNotificationView) {
    return ({

        open: function (collection) {
            var schemaCollection = new NotificationCollection();
            var orginalUrl = schemaCollection.url;
            schemaCollection.url = schemaCollection.url + '/schema';
            schemaCollection.fetch();
            schemaCollection.url = orginalUrl;

            var view = new AddSelectionNotificationView({ collection: schemaCollection, notificationCollection: collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
