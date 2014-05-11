'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/Notifications/NotificationEditView'

], function (AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/NotificationItemViewTemplate',
        tagName : 'li',

        events: {
            'click'  : '_editNotification'
        },

        initialize: function () {
            this.listenTo(this.model, 'sync', this.render);
        },

        _editNotification: function () {
            var view = new EditView({ model: this.model, notificationCollection: this.model.collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
