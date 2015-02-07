var vent = require('vent');
var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');

module.exports = Marionette.AppRouter.extend({
    initialize         : function(){
        vent.on(vent.Commands.OpenControlPanelCommand, this._openControlPanel, this);
        vent.on(vent.Commands.CloseControlPanelCommand, this._closeControlPanel, this);
    },
    _openControlPanel  : function(view){
        AppLayout.controlPanelRegion.show(view);
    },
    _closeControlPanel : function(){
        AppLayout.controlPanelRegion.closePanel();
    }
});