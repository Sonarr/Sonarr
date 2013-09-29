﻿'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AutoComplete'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/FileManagement/FileManagementViewTemplate',

            ui: {
                recyclingBin: '.x-path'
            },

            onShow: function () {
                this.ui.recyclingBin.autoComplete('/directories');
            }
        });

        return AsModelBoundView.call(view);
    });
