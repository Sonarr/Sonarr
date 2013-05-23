"use strict";

define([
    'app',
    'Settings/Notifications/Collection',
    'Settings/Notifications/EditView'

], function () {

    NzbDrone.Settings.Notifications.ItemView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/ItemTemplate',
        tagName: 'tr',

        events: {
            'click .x-edit'  : 'edit',
            'click .x-remove': 'removeNotification'
        },

        edit: function () {
            var view = new NzbDrone.Settings.Notifications.EditView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        },

        removeNotification: function () {
            var view = new NzbDrone.Settings.Notifications.DeleteView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        }
    });
});
