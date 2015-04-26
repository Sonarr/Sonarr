var Marionette = require('marionette');
var ModalRegion = require('./Shared/Modal/ModalRegion');
var FileBrowserModalRegion = require('./Shared/FileBrowser/FileBrowserModalRegion');
var ControlPanelRegion = require('./Shared/ControlPanel/ControlPanelRegion');

var Layout = Marionette.Layout.extend({
    regions : {
        navbarRegion : '#nav-region',
        mainRegion   : '#main-region'
    },

    initialize : function() {
        this.addRegions({
            modalRegion            : ModalRegion,
            fileBrowserModalRegion : FileBrowserModalRegion,
            controlPanelRegion     : ControlPanelRegion
        });
    }
});
module.exports = new Layout({ el : 'body' });