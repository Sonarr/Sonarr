"use strict";

define([
    'app',
    'Settings/Indexers/Model'

], function () {

    NzbDrone.Settings.Indexers.EditView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Indexers/EditTemplate',

        events: {
            'click .x-save': 'save'
        },

        save: function () {
            this.model.save();

//            window.alert('saving');
//            this.model.save(undefined, this.syncNotification("Notification Settings Saved", "Couldn't Save Notification Settings"));
        },


        syncNotification: function (success, error) {
            return {
                success: function () {
                    window.alert(success);
                },

                error: function () {
                    window.alert(error);
                }
            };
        }
    });
});
