"use strict";

define([
    'app',
    'Settings/Notifications/Model'

], function () {

    NzbDrone.Settings.Notifications.AddItemView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Notifications/AddItemTemplate',
        tagName  : 'li',

        events: {
            'click': 'addNotification'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        addNotification: function () {
            this.model.set('id', undefined);
            this.model.set('name', '');
            var view = new NzbDrone.Settings.Notifications.EditView({ model: this.model, notificationCollection: this.notificationCollection });
            NzbDrone.modalRegion.show(view);
        }
    });

    NzbDrone.Settings.Notifications.AddView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Notifications.AddItemView,
        itemViewContainer       : '.notifications .items',
        template                : 'Settings/Notifications/AddTemplate',

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
