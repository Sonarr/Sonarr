'use strict';
define([
    'app',
    'marionette',
    'Settings/Notifications/ItemView',
    'Settings/Notifications/SchemaModal',
    'Settings/Notifications/AddCardView'
], function (App, Marionette, NotificationItemView, SchemaModal, AddCardView) {
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
