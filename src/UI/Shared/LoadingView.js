'use strict';
define(
    [
        'marionette'
    ], function (Marionette) {
        return Marionette.ItemView.extend({
            template : 'Shared/LoadingViewTemplate',
            className: 'nz-loading row'
        });
    });
