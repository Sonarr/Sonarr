'use strict';

define([
    'marionette',
    'Settings/Notifications/NotificationItemView',
    'Settings/Notifications/Add/NotificationSchemaModal'
], function (Marionette, ItemView, SchemaModal) {
    return Marionette.CompositeView.extend({
        itemView         : ItemView,
        itemViewContainer: '.notification-list',
        template         : 'Settings/Notifications/NotificationCollectionViewTemplate',

        ui: {
            'addCard': '.x-add-card'
        },

        events: {
            'click .x-add-card': '_openSchemaModal'
        },

        appendHtml: function (collectionView, itemView, index) {
            collectionView.ui.addCard.parent('li').before(itemView.el);
        },

        _openSchemaModal: function () {
            SchemaModal.open(this.collection);
        }
    });
});
