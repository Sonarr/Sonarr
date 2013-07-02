﻿'use strict';

define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'bootstrap'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template : 'Settings/DownloadClient/BlackholeViewTemplate'
        });

        return AsModelBoundView.call(view);
    });
