'use strict';
define(
    [
        'app',
        'marionette'
    ], function (App, Marionette) {

        return  Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/DeleteTemplate',

            events: {
                'click .x-confirm-delete': '_removeProfile'
            },

            _removeProfile: function () {

                this.model.destroy({
                    wait: true
                }).done(function () {
                        App.modalRegion.close();
                    });
            }
        });
    });
