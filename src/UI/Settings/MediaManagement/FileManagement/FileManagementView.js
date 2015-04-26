var vent = require('vent');
var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
require('../../../Mixins/DirectoryAutoComplete');
require('../../../Mixins/FileBrowser');

var view = Marionette.ItemView.extend({
    template : 'Settings/MediaManagement/FileManagement/FileManagementViewTemplate',

    ui : {
        recyclingBin : '.x-path'
    },

    onShow : function() {
        this.ui.recyclingBin.fileBrowser();
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;