'use strict';

define(
    [
        'app',
        'marionette'
    ], function (App, Marionette) {
        return Marionette.ItemView.extend({
            template: 'System/Update/UpdateItemViewTemplate',

            events: {
                'click .x-install-update': '_installUpdate'
            },

            _installUpdate: function () {
                this.model.save();
            }
        });
    });
