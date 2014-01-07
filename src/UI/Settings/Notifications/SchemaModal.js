'use strict';
define([
    'AppLayout',
    'Settings/Notifications/Collection',
    'Settings/Notifications/AddView',
    'System/StatusModel'
], function (AppLayout, NotificationCollection, AddSelectionNotificationView, StatusModel) {
    return ({

        open: function (collection) {
            var schemaCollection = new NotificationCollection();
            schemaCollection.url = StatusModel.get('urlBase') + '/api/notification/schema';
            schemaCollection.fetch();
            schemaCollection.url = StatusModel.get('urlBase') + '/api/notification';

            var view = new AddSelectionNotificationView({ collection: schemaCollection, notificationCollection: collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
