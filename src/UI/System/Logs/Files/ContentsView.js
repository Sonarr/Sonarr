'use strict';

define(
    [
        'marionette'
    ], function (Marionette) {
        return Marionette.ItemView.extend({
            template: 'System/Logs/Files/ContentsViewTemplate'
        });
    });
