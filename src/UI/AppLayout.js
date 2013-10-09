define(
    [
        'marionette',
        'Shared/Modal/ModalRegion'
    ], function (Marionette, ModalRegion) {
        'use strict';

        var Layout = Marionette.Layout.extend({

            regions: {
                navbarRegion: '#nav-region',
                mainRegion  : '#main-region'
            },

            initialize: function () {
                this.addRegions({
                    modalRegion: ModalRegion
                });
            }
        });

        return new Layout({el: 'body'});
    });
