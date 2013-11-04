'use strict';
define(
    [
        'vent',
        'marionette'
    ], function (vent, Marionette) {

        return Marionette.ItemView.extend({
            template: 'System/Logs/Table/Details/LogDetailsViewTemplate'
        });
    });
