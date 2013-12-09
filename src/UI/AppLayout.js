define(
    [
        'marionette',
        'Shared/Modal/ModalRegion',
        'Shared/ControlPanel/ControlPanelRegion'
    ], function (Marionette, ModalRegion, ControlPanelRegion) {
        'use strict';

        var Layout = Marionette.Layout.extend({

            regions: {
                navbarRegion      : '#nav-region',
                mainRegion        : '#main-region'
            },

            initialize: function () {
                this.addRegions({
                    modalRegion       : ModalRegion,
                    controlPanelRegion: ControlPanelRegion
                });
            }
        });

        return new Layout({el: 'body'});
    });
