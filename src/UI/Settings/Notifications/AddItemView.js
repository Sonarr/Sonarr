'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/Notifications/EditView'
], function (AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/Notifications/AddItemTemplate',
        tagName : 'li',

        events: {
            'click': 'addNotification'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        addNotification: function (e) {
            if (this.$(e.target).hasClass('icon-info-sign')) {
                return;
            }

            this.model.set({
                id: undefined,
                name: this.model.get('implementationName'),
                onGrab: true,
                onDownload: true
            });

            var editView = new EditView({ model: this.model, notificationCollection: this.notificationCollection });
            AppLayout.modalRegion.show(editView);
        }
    });
});
