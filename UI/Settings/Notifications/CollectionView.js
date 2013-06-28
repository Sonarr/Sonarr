'use strict';
define([
    'app',
    'marionette',
    'Settings/Notifications/ItemView',
    'Settings/Notifications/SchemaModal'
], function (App, Marionette, NotificationItemView, SchemaModal) {
    return Marionette.CompositeView.extend({
        itemView         : NotificationItemView,
        itemViewContainer: '.notifications',
        template         : 'Settings/Notifications/CollectionTemplate',

        events: {
            'click .x-add': '_openSchemaModal'
        },

        _openSchemaModal: function () {
            SchemaModal.open(this.collection);
        }
    });
});
