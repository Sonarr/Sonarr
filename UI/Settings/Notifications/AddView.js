'use strict';

define([
    'marionette',
    'Settings/Notifications/AddItemView'
], function (Marionette, AddItemView) {

    return Marionette.CompositeView.extend({
        itemView         : AddItemView,
        itemViewContainer: '.add-notifications .items',
        template         : 'Settings/Notifications/AddTemplate',

        itemViewOptions: function () {
            return {
                notificationCollection: this.notificationCollection
            };
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        }
    });
});
