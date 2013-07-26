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

        ui: {
            'addCard': '.x-add-card'
        },

        events: {
            'click .x-add': '_openSchemaModal',
            'click .x-add-card': '_openSchemaModal'
        },

        onBeforeItemAdded: function () {
            this.ui.addCard.remove();
        },

        onAfterItemAdded: function () {
            this._appendAddCard();
        },

        _openSchemaModal: function () {
            SchemaModal.open(this.collection);
        },

        _appendAddCard: function () {
            this.$itemViewContainer.find('.x-add-card').remove();

            this.templateFunction = Marionette.TemplateCache.get('Settings/Notifications/AddCardTemplate');
            var html = this.templateFunction();

            this.$itemViewContainer.append(html);
        }
    });
});
