'use strict';
define([
    'marionette',
    'Settings/Notifications/ItemView',
    'Settings/Notifications/SchemaModal'
], function (Marionette, NotificationItemView, SchemaModal) {
    return Marionette.CompositeView.extend({
        itemView         : NotificationItemView,
        itemViewContainer: '.notifications',
        template         : 'Settings/Notifications/CollectionTemplate',

        ui: {
          'addCard': '.x-add-card'
        },

        events: {
            'click .x-add-card': '_openSchemaModal'
        },

        appendHtml: function(collectionView, itemView, index){
            collectionView.ui.addCard.parent('li').before(itemView.el);
        },

        _openSchemaModal: function () {
            SchemaModal.open(this.collection);
        }
    });
});
