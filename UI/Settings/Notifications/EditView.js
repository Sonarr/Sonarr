"use strict";

define([
    'app',
    'Settings/Notifications/Model',
    'Settings/Notifications/DeleteView'

], function () {

    NzbDrone.Settings.Notifications.EditView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/EditTemplate',

        events: {
            'click .x-save': '_saveNotification',
            'click .x-remove': '_deleteNotification'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        _saveNotification: function () {
            var name = this.model.get('name');
            var success = 'Notification Saved: ' + name;
            var fail = 'Failed to save notification: ' + name;

            this.model.save(undefined, this.syncNotification(success, fail, this));
        },

        _deleteNotification: function () {
            var view = new NzbDrone.Settings.Notifications.DeleteView({ model: this.model });
            NzbDrone.modalRegion.show(view);
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
