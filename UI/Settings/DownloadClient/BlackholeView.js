﻿'use strict';

define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AutoComplete',
        'bootstrap'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template : 'Settings/DownloadClient/BlackholeViewTemplate',

            ui: {
                'blackholeFolder': '.x-path'
            },

            onShow: function () {
                this.ui.blackholeFolder.autoComplete('/directories');
            }
        });

        return AsModelBoundView.call(view);
    });
