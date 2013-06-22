'use strict';

define([
    'app',
    'marionette',
    'Settings/Notifications/EditView',
    'Settings/Notifications/DeleteView'

], function (App, Marionette, EditView, DeleteView) {

    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/ItemTemplate',
        tagName : 'tr',

        events: {
            'click .x-edit'  : 'edit',
            'click .x-delete': 'deleteNotification'
        },

        edit: function () {
            var view = new EditView({ model: this.model, notificationCollection: this.model.collection});
            App.modalRegion.show(view);
        },

        deleteNotification: function () {
            var view = new DeleteView({ model: this.model});
            App.modalRegion.show(view);
        }
    });
});
