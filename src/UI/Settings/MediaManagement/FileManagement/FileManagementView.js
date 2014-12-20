'use strict';
define(
    [
        'vent',
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/DirectoryAutoComplete',
        'Mixins/FileBrowser'
    ], function (vent, Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/FileManagement/FileManagementViewTemplate',

            ui: {
                recyclingBin : '.x-path'
            },

            onShow: function () {
                this.ui.recyclingBin.fileBrowser();
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
