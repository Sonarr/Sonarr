define(
    [
        'marionette',
        'Shared/Modal/ModalRegion',
        'Shared/FileBrowser/FileBrowserModalRegion',
        'Shared/ControlPanel/ControlPanelRegion'
    ], function (Marionette, ModalRegion, FileBrowserModalRegion, ControlPanelRegion) {
        'use strict';

        var Layout = Marionette.Layout.extend({

            regions: {
                navbarRegion      : '#nav-region',
                mainRegion        : '#main-region'
            },

            initialize: function () {
                this.addRegions({
                    modalRegion            : ModalRegion,
                    fileBrowserModalRegion : FileBrowserModalRegion,
                    controlPanelRegion     : ControlPanelRegion
                });
            }
        });

        return new Layout({el: 'body'});
    });
