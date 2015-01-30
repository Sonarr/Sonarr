'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/FileBrowser'
    ], function (Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/DownloadClient/DroneFactory/DroneFactoryViewTemplate',

            ui: {
                droneFactory : '.x-path'
            },

            onShow: function () {
                this.ui.droneFactory.fileBrowser();
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
