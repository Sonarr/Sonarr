"use strict";

define([
    'app',
    'Settings/Notifications/Model'

], function () {

    NzbDrone.Settings.Notifications.AddItemView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Notifications/AddItemTemplate',
        tagName  : 'button',
        className: 'btn',

        events: {
            'click': 'add'
        },

        add: function () {
            this.model.set('id', undefined);
            var view = new NzbDrone.Settings.Notifications.EditView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        }
    });

    NzbDrone.Settings.Notifications.AddView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Notifications.AddItemView,
        itemViewContainer       : '#notifications-to-add',
        template                : 'Settings/Notifications/AddTemplate'
    });
});
