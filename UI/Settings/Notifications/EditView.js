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

            this.model.save(undefined, NzbDrone.Settings.SyncNotificaiton.callback({
                successMessage: success,
                errorMessage: fail,
                successCallback: this._saveSuccess,
                context: this
            }));
        },

        _deleteNotification: function () {
            var view = new NzbDrone.Settings.Notifications.DeleteView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        },

        _saveSuccess: function () {
            this.notificationCollection.add(this.model, { merge: true });
            NzbDrone.modalRegion.closeModal();
        }
    });
});
