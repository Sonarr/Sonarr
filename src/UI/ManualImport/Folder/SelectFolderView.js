var Marionette = require('marionette');
require('../../Mixins/FileBrowser');

module.exports = Marionette.ItemView.extend({
    template : 'ManualImport/Folder/SelectFolderViewTemplate',

    ui : {
        path    : '.x-path',
        buttons : '.x-button'
    },

    events: {
        'click .x-manual-import'    : '_manualImport',
        'click .x-automatic-import' : '_automaticImport',
        'change .x-path'            : '_updateButtons',
        'keyup .x-path'              : '_updateButtons'
    },

    onRender : function() {
        this.ui.path.fileBrowser();
        this._updateButtons();
    },

    path : function() {
        return this.ui.path.val();
    },

    _manualImport : function () {
        if (this.ui.path.val()) {
            this.trigger('manualImport', { folder: this.ui.path.val() });
        }
    },

    _automaticImport : function () {
        if (this.ui.path.val()) {
            this.trigger('automaticImport', { folder: this.ui.path.val() });
        }
    },

    _updateButtons : function () {
        if (this.ui.path.val()) {
            this.ui.buttons.removeAttr('disabled');
        }

        else {
            this.ui.buttons.attr('disabled', 'disabled');
        }
    }
});
