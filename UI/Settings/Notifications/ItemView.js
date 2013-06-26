'use strict';

define([
    'app',
    'marionette',
    'Settings/Notifications/EditView',
    'Settings/Notifications/DeleteView'

], function (App, Marionette, EditView, DeleteView) {

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
            App.modalRegion.show(view);
        },

        _deleteNotification: function () {
            var view = new DeleteView({ model: this.model});
            App.modalRegion.show(view);
        }
    });
});
