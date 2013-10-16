'use strict';
define(
    [
        'vent',
        'marionette'
    ], function (vent, Marionette) {

        return  Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/DeleteTemplate',

            events: {
                'click .x-confirm-delete': '_removeProfile'
            },

            _removeProfile: function () {

                this.model.destroy({
                    wait: true
                }).done(function () {
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });
            }
        });
    });
