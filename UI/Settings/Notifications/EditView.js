"use strict";

define([
    'app',
    'Settings/Notifications/Model'

], function () {

    NzbDrone.Settings.Notifications.EditView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/EditTemplate',

        events: {
            'click .x-save': 'save'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        save: function () {
            var name = this.model.get('name');
            var success = 'Notification Saved: ' + name;
            var fail = 'Failed to save notification: ' + name;

            this.model.save(undefined, this.syncNotification(success, fail, this));
        },

        syncNotification: function (success, error, context) {
            return {
                success: function () {
                    NzbDrone.Shared.Messenger.show({
                        message: success
                    });

                    context.notificationCollection.add(context.model, { merge: true });
                    NzbDrone.modalRegion.closeModal();
                },

                error: function () {
                    window.alert(error);
                }
            };
        }
    });
});
