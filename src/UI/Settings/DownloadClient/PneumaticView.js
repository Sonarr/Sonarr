﻿'use strict';

define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AutoComplete',
        'bootstrap'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template : 'Settings/DownloadClient/PneumaticViewTemplate',

            ui: {
                'pneumaticFolder': '.x-path'
            },

            onShow: function () {
                this.ui.pneumaticFolder.autoComplete('/directories');
            }
        });

        return AsModelBoundView.call(view);
    });
