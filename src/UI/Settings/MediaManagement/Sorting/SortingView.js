var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var view = Marionette.ItemView.extend({
    template : 'Settings/MediaManagement/Sorting/SortingViewTemplate',

    events : {
        'change .x-import-extra-files' : '_setExtraFileExtensionVisibility'
    },

    ui : {
        importExtraFiles    : '.x-import-extra-files',
        extraFileExtensions : '.x-extra-file-extensions'
    },

    onRender : function() {
        if (!this.ui.importExtraFiles.prop('checked')) {
            this.ui.extraFileExtensions.hide();
        }
    },

    _setExtraFileExtensionVisibility : function() {
        var showExtraFileExtensions = this.ui.importExtraFiles.prop('checked');

        if (showExtraFileExtensions) {
            this.ui.extraFileExtensions.slideDown();
        }

        else {
            this.ui.extraFileExtensions.slideUp();
        }
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
