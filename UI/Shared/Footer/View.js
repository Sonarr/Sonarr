"use strict";
define(
    [
        'marionette',
        'System/StatusModel'
    ], function (Marionette, StatusModel) {
        return Marionette.ItemView.extend({

            template: 'Shared/Footer/Template',
            model   : StatusModel
        });
    });
