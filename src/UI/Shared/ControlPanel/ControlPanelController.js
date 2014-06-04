'use strict';
define(
    [
        'vent',
        'AppLayout',
        'marionette'
    ], function (vent, AppLayout, Marionette) {

        return Marionette.AppRouter.extend({

            initialize: function () {
                vent.on(vent.Commands.OpenControlPanelCommand, this._openModal, this);
                vent.on(vent.Commands.CloseControlPanelCommand, this._closeModal, this);
            },

            _openModal: function (view) {
                AppLayout.controlPanelRegion.show(view);
            },

            _closeModal: function () {
                AppLayout.controlPanelRegion.closePanel();
            }
        });
    });
