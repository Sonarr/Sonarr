﻿'use strict';
define(
    [
        'marionette'
    ], function (Marionette) {
        return Marionette.ItemView.extend({
            template : 'Shared/LoadingTemplate',
            className: 'nz-loading row'
        });
    });
