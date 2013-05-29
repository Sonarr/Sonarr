"use strict";

define([
    'app',
    'Settings/Notifications/Collection',
    'Settings/Notifications/EditView',
    'Settings/Notifications/DeleteView'

], function () {

    NzbDrone.Settings.Notifications.ItemView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/ItemTemplate',
        tagName: 'tr',

        events: {
            'click .x-edit'  : 'edit',
            'click .x-delete': 'deleteNotification'
        },

        edit: function () {
            var view = new NzbDrone.Settings.Notifications.EditView({ model: this.model, notificationCollection: this.model.collection});
            NzbDrone.modalRegion.show(view);
        },

        deleteNotification: function () {
            var view = new NzbDrone.Settings.Notifications.DeleteView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        }
    });
});
