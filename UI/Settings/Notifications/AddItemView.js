"use strict";

define([
    'app',
    'marionette',
    'Settings/Notifications/EditView'
], function (App, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/AddItemTemplate',
        tagName : 'li',

        events: {
            'click': 'addNotification'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        addNotification: function () {
            this.model.set('id', undefined);
            var editView = new EditView({ model: this.model, notificationCollection: this.notificationCollection });
            App.modalRegion.show(editView);
        }
    });
});
