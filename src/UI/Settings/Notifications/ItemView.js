'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/Notifications/NotificationEditView',
    'Settings/Notifications/DeleteView'

], function (AppLayout, Marionette, EditView, DeleteView) {

    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/ItemTemplate',
        tagName : 'li',

        events: {
            'click .x-edit'  : '_editNotification',
            'click .x-delete': '_deleteNotification'
        },

        initialize: function () {
            this.listenTo(this.model, 'sync', this.render);
        },

        _editNotification: function () {
            var view = new EditView({ model: this.model, notificationCollection: this.model.collection});
            AppLayout.modalRegion.show(view);
        },

        _deleteNotification: function () {
            var view = new DeleteView({ model: this.model});
            AppLayout.modalRegion.show(view);
        }
    });
});
