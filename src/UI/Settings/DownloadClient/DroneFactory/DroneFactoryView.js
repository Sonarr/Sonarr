var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
require('../../../Mixins/FileBrowser');

var view = Marionette.ItemView.extend({
    template : 'Settings/DownloadClient/DroneFactory/DroneFactoryViewTemplate',

    ui : {
        droneFactory : '.x-path'
    },

    onShow : function() {
        this.ui.droneFactory.fileBrowser();
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;