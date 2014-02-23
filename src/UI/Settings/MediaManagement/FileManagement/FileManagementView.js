'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/AutoComplete'
    ], function (Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/FileManagement/FileManagementViewTemplate',

            ui: {
                recyclingBin : '.x-path'
            },

            onShow: function () {
                this.ui.recyclingBin.autoComplete('/directories');
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
