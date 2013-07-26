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
            'click .x-add-card': '_openSchemaModal'
        },

        onRender: function () {
            this.listenTo(this.collection, 'add', this.render);

            this.templateFunction = Marionette.TemplateCache.get('Settings/Notifications/AddCardTemplate');
            var html = this.templateFunction();

            this.$itemViewContainer.append(html);
        },

        _openSchemaModal: function () {
            SchemaModal.open(this.collection);
        }
    });
});
