'use strict';

define(
    [
        'marionette',
        'Commands/CommandController'
    ], function (Marionette, CommandController) {
        return Marionette.ItemView.extend({
            template: 'System/Update/UpdateItemViewTemplate',

            events: {
                'click .x-install-update': '_installUpdate'
            },

            _installUpdate: function () {
                CommandController.Execute('installUpdate', { updatePackage: this.model.toJSON() });
            }
        });
    });
