var Marionette = require('marionette');
var ModalRegion = require('./Shared/Modal/ModalRegion');
var FileBrowserModalRegion = require('./Shared/FileBrowser/FileBrowserModalRegion');
var ControlPanelRegion = require('./Shared/ControlPanel/ControlPanelRegion');

module.exports = (function(){
    'use strict';
    var Layout = Marionette.Layout.extend({
        regions    : {
            navbarRegion : '#nav-region',
            mainRegion   : '#main-region'
        },
        initialize : function(){
            this.addRegions({
                modalRegion            : ModalRegion,
                fileBrowserModalRegion : FileBrowserModalRegion,
                controlPanelRegion     : ControlPanelRegion
            });
        }
    });
    return new Layout({el : 'body'});
}).call(this);