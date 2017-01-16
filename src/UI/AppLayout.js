var Marionette = require('marionette');
var ModalRegion = require('./Shared/Modal/ModalRegion');
var ModalRegion2 = require('./Shared/Modal/ModalRegion2');
var ControlPanelRegion = require('./Shared/ControlPanel/ControlPanelRegion');

var Layout = Marionette.Layout.extend({
    regions : {
        navbarRegion : '#nav-region',
        mainRegion   : '#main-region'
    },

    initialize : function() {
        this.addRegions({
            modalRegion        : ModalRegion,
            modalRegion2       : ModalRegion2,
            controlPanelRegion : ControlPanelRegion
        });
    }
});
module.exports = new Layout({ el : 'body' });