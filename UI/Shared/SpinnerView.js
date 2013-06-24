﻿'use strict';

define(
    [
        'marionette'
    ], function (Marionette) {
        return Marionette.ItemView.extend({
            template : 'Shared/SpinnerTemplate',
            className: 'nz-spinner row'
        });
    });


