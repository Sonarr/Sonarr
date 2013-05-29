'use strict';
define(['app', 'Settings/Notifications/Model'], function () {

    NzbDrone.Settings.Notifications.DeleteView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Notifications/DeleteTemplate',

        events: {
            'click .x-confirm-delete': 'removeNotification'
        },

        removeNotification: function () {
            var self = this;

            //Success is not getting triggered: http://stackoverflow.com/questions/6988873/backbone-model-destroy-not-triggering-success-function-on-success
            this.model.destroy({
                wait   : true,
                success: function (model) {
                    model.collection.remove(model);
                    self.$el.parent().modal('hide');
                }
            });
        }
    });
});
