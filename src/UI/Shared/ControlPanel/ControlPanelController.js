'use strict';
define(
    [
        'vent',
        'AppLayout',
        'marionette'
    ], function (vent, AppLayout, Marionette) {

        return Marionette.AppRouter.extend({

            initialize: function () {
                vent.on(vent.Commands.OpenControlPanelCommand, this._openControlPanel, this);
                vent.on(vent.Commands.CloseControlPanelCommand, this._closeControlPanel, this);
            },

            _openControlPanel: function (view) {
                AppLayout.controlPanelRegion.show(view);
            },

            _closeControlPanel: function () {
                AppLayout.controlPanelRegion.closePanel();
            }
        });
    });
